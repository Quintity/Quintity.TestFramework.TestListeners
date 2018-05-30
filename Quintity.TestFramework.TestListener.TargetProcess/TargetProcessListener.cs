using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using Quintity.TestFramework.Core;
using Quintity.TestFramework.Runtime;

namespace Quintity.TestFramework.TestListeners
{
    enum TestCaseStatus
    {
        Passed,
        Failed,
        OnHold,
        Blocked
    }

    public class TargetProcessListener : TestListener
    {

        #region fields

        readonly string targetProcessPassword = TestProperties.GetPropertyValueAsString("TargetProcessPassword");
        readonly string targetProcessBaseURL = TestProperties.GetPropertyValueAsString("TargetProcessBaseURL");
        readonly string targetProcessUser = TestProperties.GetPropertyValueAsString("TargetProcessUser");
        Dictionary<string, string> childTestPlanRunIds = new Dictionary<string, string>();
        KeyValuePair<string, string> parentTestPlanRunId = new KeyValuePair<string, string>();
        List<string> testCaseRunIds = new List<string>();
        StringBuilder testChecks = new StringBuilder();
        TestSuite initialTestSuite = null;
        string testPlanRunName = String.Empty;
        string testPlanRunId = String.Empty;
        string testCaseRunId = String.Empty;
        string testPlanId = String.Empty;
        string projectId = String.Empty;

        readonly int maxRetry = 3;

        #endregion

        #region public methods

        public override void OnTestExecutionBegin(TestExecutor testExecutor, TestExecutionBeginArgs args)
        {
            if (args.TestScriptObject is TestSuite)
            {
                initialTestSuite = args.TestScriptObject as TestSuite;
            }
        }

        public override void OnTestCaseExecutionBegin(TestCase testCase, TestCaseBeginExecutionArgs args)
        {
            Debug.Write("OnTestCaseExecutionBegin called, test case id = " + testCase.UserID + ", test plan runid = " + testPlanRunId);
            // Get the test case id from the QTF
            string testCaseId = testCase.UserID;

            // Set the correct test case run Id for each test case executed
            if (!String.IsNullOrEmpty(testCaseId))
            {
                testCaseRunId = getTestCaseRunId(testCaseId);
            }

            if (null == testPlanRunId || testPlanRunId == "")
            {
                var crap = testCase.ParentID;
            }
        }

        public override void OnTestCaseExecutionComplete(TestCase testCase, TestCaseResult testCaseResult)
        {
            Debug.Write("OnTestCaseExecutionComplete called, test case id = " + testCase.UserID + ", test plan runid = " + testPlanRunId);
            // Get test case result status for target process converted from QTF test case status
            if (!String.IsNullOrEmpty(testCase.UserID))
            {
                var result = convertFromQTFStatus(testCaseResult);

                string testCaseRunUri = targetProcessBaseURL + "/TestPlanRuns/" + testPlanRunId + "/TestCaseRuns";

                //Debug -something removing testing plan run id's
                if (null == testPlanRunId || testPlanRunId == "")
                {
                    var crap = testCase.ParentID;
                }

                // Build out test case results for target process post information
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("Test Case:  {0} - {1}", testCase.UserID, testCase.Title));
                sb.AppendLine("&lt;br /&gt;");
                sb.AppendLine(string.Format("Start Time: {0}  End Time: {1}", testCaseResult.StartTime, testCaseResult.EndTime));
                sb.AppendLine("&lt;br /&gt;");
                sb.AppendLine(testChecks.ToString());

                // Update target process with test case results
                string testCaseRunPostData = createTestCasePostData(testCaseRunId, result, sb.ToString());
                XDocument testCaseRunResponse = postRequest(testCaseRunUri, testCaseRunPostData);
            }

            // Reset test checks after writing out 
            testChecks = new StringBuilder();
        }

        public override void OnTestExecutionComplete(TestExecutor testExecutor, TestExecutionCompleteArgs args)
        {
            childTestPlanRunIds = new Dictionary<string, string>();
        }



        public override void OnTestStepExecutionComplete(TestStep testStep, TestStepResult testStepResult)
        {
            // Retrieve the test check collection from the test result
            TestCheckCollection tcc = testStepResult.TestChecks;

            // Append each test check to the target process test check result
            if (tcc != null && tcc.Count != 0)
            {
                foreach (var testCheck in tcc)
                {
                    testChecks.AppendLine(string.Format("TestCheck: {0}" + ":  Comment: {1}",
                        testCheck.Description, testCheck.Comment));
                    testChecks.AppendLine("&lt;br /&gt;");
                }
            }
        }

        public override void OnTestSuiteExecutionBegin(TestSuite testSuite, TestSuiteBeginExecutionArgs args)
        {
            Debug.Write("OnTestSuiteExecutionBegin called, test plan runid = " + testPlanRunId);
            projectId = testSuite.Project;
            testPlanId = testSuite.UserID;

            testSuiteBegin(testSuite);
        }

        public override void OnTestSuiteExecutionComplete(TestSuite testSuite, TestSuiteResult testSuiteResult)
        {
            Debug.Write("OnTestSuiteExecutionComplete called, test plan runid = " + testPlanRunId);
            //TRIAL - GET MATCHING Test run from dictionary
            if (childTestPlanRunIds.ContainsKey(testSuite.UserID) || parentTestPlanRunId.Key.Equals(testSuite.UserID))
            {
                if (testPlanRunId == "")
                {
                    testPlanRunId = parentTestPlanRunId.Value;
                }

                if (testPlanRunId == "")
                {
                    var testSuiteDetail = testSuite.UserID;
                }
                string postData = createTestPlanRunCompletePostData();
                string testSuitePostUri = targetProcessBaseURL + "/TestPlanRuns/" + testPlanRunId;
                jsonPostRequest(testSuitePostUri, postData);

                // Reset the test plan run Id and test plan run name
                Thread.Sleep(500);
                testPlanRunId = String.Empty;
                testPlanRunName = String.Empty;
            }
        }

        public override void OnTestPostprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args) { }

        public override void OnTestPostprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult) { }

        public override void OnTestPreprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args) { }

        public override void OnTestPreprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult) { }

        public override void OnTestStepExecutionBegin(TestStep testStep, TestStepBeginExecutionArgs args) { }

        public override void OnTestTrace(string virtualUser, string traceMessage) { }

        public override void OnTestWarning(TestWarning testWarning) { }

        public override void OnTestMetric(string virtualUser, TestMetricEventArgs args) { }

        public override void OnTestCheck(TestCheck testCheck) { }


        #endregion

        #region private methods

        private XDocument submitRequest(string uri)
        {
            int currentRetry = 0;

            for (; ; )
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    request.Credentials = new NetworkCredential(targetProcessUser, targetProcessPassword);

                    using (WebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                        {
                            string responseString = responseReader.ReadToEnd();
                            return XDocument.Parse(responseString);
                        }
                    }
                }
                catch (Exception e)
                {
                    currentRetry++;

                    if (currentRetry > maxRetry)
                    {
                        throw e;
                    }
                }
            }
        }

        private string submitJSONRequest(string uri)
        {
            int currentRetry = 0;

            for (; ; )
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    request.Credentials = new NetworkCredential(targetProcessUser, targetProcessPassword);

                    using (WebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                        {
                            string responseString = responseReader.ReadToEnd();
                            return responseString;
                        }
                    }
                }
                catch (Exception e)
                {
                    currentRetry++;

                    if (currentRetry > maxRetry)
                    {
                        throw e;
                    }
                }
            }
        }

        private XDocument postRequest(string uri, string postData)
        {
            int currentRetry = 0;

            for (; ; )
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    request.Credentials = new NetworkCredential(targetProcessUser, targetProcessPassword);
                    request.Method = WebRequestMethods.Http.Post;
                    request.ContentLength = postData.Length;

                    //Trial test
                    if (uri.Contains("/TestPlanRuns//"))
                    { }

                    using (StreamWriter postStream = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII))
                    {
                        postStream.Write(postData);
                        postStream.Close();

                        using (WebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                            {
                                string postResponse = responseReader.ReadToEnd();
                                return XDocument.Parse(postResponse);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    currentRetry++;

                    if (currentRetry > maxRetry)
                    {
                        throw e;
                    }
                }
            }
        }

        private string jsonPostRequest(string uri, string postData)
        {
            int currentRetry = 0;

            for (; ; )
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    request.Credentials = new NetworkCredential(targetProcessUser, targetProcessPassword);
                    request.ContentType = "application/json";
                    request.Method = WebRequestMethods.Http.Post;

                    request.ContentLength = postData.Length;

                    using (StreamWriter postStream = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII))
                    {
                        postStream.Write(postData);
                        postStream.Close();

                        using (WebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                            {
                                string postResponse = responseReader.ReadToEnd();
                                return postResponse;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    currentRetry++;

                    if (currentRetry > maxRetry)
                    {
                        throw e;
                    }
                }
            }
        }

        private void testSuiteBegin(TestSuite testSuite)
        {
            if (!testSuite.Status.ToString().Equals("Inactive"))
            {
                // Crete a test plan run if needed
                createTestPlanRun(testSuite);

                // Recursively get all child test plans from master test plan
                if (childTestPlanRunIds.Count == 0)
                {
                    getChildTestPlanRunIds(testPlanRunId);
                }

                // Get the target process test plan run ID from the dictionary that matches the QTF test suite defined test plan id
                if (childTestPlanRunIds.ContainsKey(testSuite.UserID))
                {
                    testPlanRunId = childTestPlanRunIds[testSuite.UserID];
                    if (testPlanRunId == "")
                    { }

                    testCaseRunIds = getTestCaseRunIds(testPlanRunId);
                }

                if (parentTestPlanRunId.Equals(default(KeyValuePair<string, string>)))
                {
                    parentTestPlanRunId = new KeyValuePair<string, string>(testPlanId, testPlanRunId);
                }

            }
        }

        private void createTestPlanRun(TestSuite testSuite)
        {
            // Check to ensure a project id and plan id was found as well as no test plan run name has been
            // previsouly created, otherwise dont create a new plan run
            if (!projectId.Equals(String.Empty) && !testPlanId.Equals(String.Empty) && testPlanRunName.Equals(string.Empty))
            {
                //Create a test plan run name with a prefix and a data time stamp
                testPlanRunName = string.Format("{0} " + "- {1}", testSuite.Title, DateTime.Now.ToString());

                // Create a new test plan run
                string testPlanRunUri = targetProcessBaseURL + "/TestPlanRuns";
                string testPlanRunPostData = createTestPlanRunPostData(testPlanRunName, projectId, testPlanId);
                XDocument testPlanRunResponse = postRequest(testPlanRunUri, testPlanRunPostData);

                //Set parent test plan Id
                testPlanRunId = testPlanRunResponse.Element("TestPlanRun").Attribute("Id").Value;
            }
        }

        private string getTestPlanRunId(string testPlanRunName)
        {
            string uri = targetProcessBaseURL + "/TestPlanRuns?where=(Name eq '" + testPlanRunName + "')";
            XDocument doc = submitRequest(uri);
            string testPlanRunId = doc.Element("TestPlanRuns").Element("TestPlanRun").Attribute("Id").Value;
            return testPlanRunId;
        }

        private void getChildTestPlanRunIds(string testPlanRunId)
        {
            string uri = "https://cognosante.tpondemand.com//api/v1/TestPlanRuns/" + testPlanRunId + "/TestPlanRuns";

            XDocument doc = submitRequest(uri);

            foreach (var testPlanRun in doc.Elements())
            {
                foreach (var subTestPlanRun in testPlanRun.Elements("TestPlanRun"))
                {
                    string childTestPlanRunId = subTestPlanRun.Attribute("Id").Value;
                    string childTestPlanId = subTestPlanRun.Element("TestPlan").Attribute("Id").Value;

                    childTestPlanRunIds.Add(childTestPlanId, childTestPlanRunId);

                    getChildTestPlanRunIds(childTestPlanRunId);
                }
            }
        }

        private string getTestCaseRunId(string testCaseId)
        {
            string uri = targetProcessBaseURL + "/TestPlanRuns/" + testPlanRunId + "/TestCaseRuns?take=1000";
            XDocument doc = submitRequest(uri);

            if (doc != null)
            {
                foreach (var testCaseRuns in doc.Elements())
                {
                    foreach (var testCaseRun in testCaseRuns.Elements("TestCaseRun"))
                    {
                        string testCaseIdValue = testCaseRun.Element("TestCase").Attribute("Id").Value;
                        if (testCaseIdValue.Equals(testCaseId))
                        {
                            return testCaseRun.Attribute("Id").Value;
                        }
                    }
                }
            }

            return null;
        }

        private List<string> getTestCaseRunIds(string testPlanRunId)
        {
            List<string> testCaseRunIds = new List<string>();
            string uri = targetProcessBaseURL + "/TestPlanRuns/" + testPlanRunId + "/TestCaseRuns";
            XDocument doc = submitRequest(uri);

            if (doc != null)
            {
                foreach (var testCaseRuns in doc.Elements())
                {
                    foreach (var testCaseRun in testCaseRuns.Elements("TestCaseRun"))
                    {
                        testCaseRunIds.Add(testCaseRun.Attribute("Id").Value);
                    }
                }

                return testCaseRunIds;
            }

            return null;
        }

        // Leaving in for possible future use
        private List<string> getTestStepRunIds(string testCaseRunId)
        {
            List<string> testStepRunIds = new List<string>();
            string uri = targetProcessBaseURL + "/TestCaseRuns/" + testCaseRunId + "/TestStepRuns";
            XDocument doc = submitRequest(uri);

            if (doc != null)
            {
                foreach (var testStepRuns in doc.Elements())
                {
                    foreach (var testStepRun in testStepRuns.Elements("TestStepRun"))
                    {
                        testStepRunIds.Add(testStepRun.Attribute("Id").Value);
                    }
                }

                return testStepRunIds;
            }

            return null;
        }

        // Leaving in for possible future use
        private void updateTestStep(string testStepRunId, bool result)
        {
            string uri = targetProcessBaseURL + "/TestStepRuns/" + testStepRunId;
            string postData = createTestStepPostData(result);

            postRequest(uri, postData);
        }

        private string createTestStepPostData(bool result)
        {

            string postData =
                "<TestStepRun>" +
                "<Passed>" + result.ToString() + "</Passed>" +
                "<Runned>" + true.ToString() + "</Runned>" +
                "</TestStepRun>";

            return postData;
        }

        // Leaving in for possible future use
        private string createTestCasePostData(string testCaseRunId, TestCaseStatus status)
        {

            string postData =
                    "<TestCaseruns>" +
                    "<TestCaseRun>" +
                    "<Id>" + testCaseRunId + "</Id>" +
                    "<Status>" + status.ToString() + "</Status>" +
                    "</TestCaseRun>" +
                    "</TestCaseruns>";

            return postData;
        }

        private string createTestCasePostData(string testCaseRunId, TestCaseStatus status, string comment)
        {

            string postData =
                    "<TestCaseruns>" +
                    "<TestCaseRun>" +
                    "<Id>" + testCaseRunId + "</Id>" +
                    "<Status>" + status.ToString() + "</Status>" +
                    "<Comment>" + comment + "</Comment>" +
                    "</TestCaseRun>" +
                    "</TestCaseruns>";

            return postData;
        }

        private string createTestPlanRunPostData(string testPlanRunName, string projectId, string testPlanId)
        {
            string postData =
                    "<TestPlanRun Name=\"" + testPlanRunName + "\">" +
                    "<Project Id =\"" + projectId + "\"/>" +
                    "<TestPlan Id=\"" + testPlanId + "\"/>" +
                    "</TestPlanRun>";

            return postData;
        }

        private string createTestPlanRunUpdatePostData(string testPlanRunName, string projectId, string testPlanId, string test)
        {
            string postData =
                    "<TestPlanRun>" +
                    "<Id>" + testPlanRunId + "</Id>" +
                    "<Name>" + testPlanRunId + "</Id>" +
                    "<Project Id =\"" + projectId + "\"/>" +
                    "<TestPlan Id=\"" + testPlanId + "\"/>" +
                    "</TestPlanRun>";

            return postData;
        }

        private string createTestPlanRunCompletePostData()
        {
            if (testPlanRunId != null && testPlanRunId != "")
            {
                //Get Process ID for entity
                string testPlanEntityURL = targetProcessBaseURL + "/TestPlanRuns/" + testPlanRunId + "?include=[Project[Process[ID]]]&format=json";
                string procIdResponse = submitJSONRequest(testPlanEntityURL);
                JObject procIdJSON = JObject.Parse(procIdResponse);
                var procID = (string)procIdJSON["Project"]["Process"]["Id"];

                //Get Entity State Id
                testPlanEntityURL = targetProcessBaseURL + "/EntityStates?include=[Id,Name]&where=(EntityType.Name eq \"TestPlanRun\") and (Workflow.Process.ID eq \"" + procID + "\")&format=json";
                string entityStateIdResponse = submitJSONRequest(testPlanEntityURL);
                JObject EntityStateIdJSON = JObject.Parse(entityStateIdResponse);
                var EntityStateId = EntityStateIdJSON["Items"][1]["Id"];

                //Format and return post data
                string postData = @"{ 'EntityState': { 'ID': " + EntityStateId + ", 'Name': 'Done'}}";
                return postData;
            }

            return null;
        }

        private TestCaseStatus convertFromQTFStatus(TestCaseResult testCaseResult)
        {
            if (testCaseResult.TestVerdict.ToString().Equals("Fail"))
            {
                return TestCaseStatus.Failed;
            }
            else if (testCaseResult.TestVerdict.ToString().Equals("Pass"))
            {
                return TestCaseStatus.Passed;
            }
            else
            {
                return TestCaseStatus.Blocked;
            }
        }

        #endregion
    }
}
