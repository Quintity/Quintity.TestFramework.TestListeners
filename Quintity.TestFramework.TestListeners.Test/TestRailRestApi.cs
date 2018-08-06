using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Quintity.TestFramework.Core;
using TestRail;


namespace Quintity.TestFramework.TestListeners.Tests
{
    [TestClass]
    public class TestRailRestApi : TestClassBase
    {
        [TestMethod]
        public TestVerdict GetProjects(
            [TestParameter("TestRail Url", "Enter the TestRail Url", "[TestRailUrl]")]
            string url,
            [TestParameter("User name", "Enter the TestRail user name.", "[TestRailUser]")]
            string userName,
            [TestParameter("User password", "Enter the TestRail user password.", "[TestRailPassword]")]
            string password)
        {
            try
            {
                var client = new TestRail.TestRailClient(url, userName, password);

                var projects = client.GetProjects();

                var project = projects.Find(x => x.Name.Equals("TestProject"));

                //client.GetProject()

                var runs = client.GetRuns(project.ID);

                var bob = client.GetResults(136);

                var results = client.GetResultsForCase(59, 136);

                var suites = client.GetSuites(project.ID);
                var suite = suites.Find(x => x.Name.Equals("Master test suite"));


                var testRun = client.AddRun(project.ID, (ulong)suite.ID, $"Very important test run:  {DateTime.Now}", "Blah, blah, blah", 1);
                var dorks = client.GetResultsForRun(60);

                var tests = client.GetTests(60);
                var cases = client.GetCases(project.ID, (ulong)suite.ID);

                var comment = File.ReadAllText(@"C:\DevProjects\Quintity\Repos\Quintity.TestFramework.TestListeners\Quintity.TestFramework.TestListeners.Test\TestProperties\TestProperties.props");

                foreach (var @case in cases)
                {
                    var spud = client.AddResultForCase(testRun.Value, (ulong)@case.ID, TestRail.Types.ResultStatus.Passed, comment, "version 1.0", new TimeSpan(0, 0, 30));
                    TestTrace.Trace($"Added result:  {spud.Value}");
                }

                TestMessage += "Success";
                TestVerdict = TestVerdict.Pass;

            }
            catch (Exception e)
            {
                TestMessage += e.ToString();
                TestVerdict = TestVerdict.Error;
            }

            return TestVerdict.Pass;
        }

        /*
                #region Data members

                #endregion

                #region Test methods
                [TestMethod("Get projects", "This project returns the projects.")]
                public TestVerdict GetProjects(
                    [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                    [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                    [TestParameter("Password", "Enter user password", "[Password]")]
                    string password
                    )
                {
                    try
                    {
                        var requestUri = $"{endpoint}/index.php?/api/v2/get_projects";
                        var body = getResponseBody(getRequest(requestUri, userName, password));

                        var projects = Project.CreateList(body);
                        var project = projects.Find(x => x.Name.Equals("B2B"));

                        TestMessage += body;
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                [TestMethod("Get project", "This project returns the named project.")]
                public TestVerdict GetProject(
                    [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                    [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                    [TestParameter("Password", "Enter user password", "[Password]")]
                    string password,
                    [TestParameter("Project Name", "Enter Project Name")]
                    string projectName
                    )
                {
                    try
                    {
                        var projects = getProjects(endpoint, userName, password);

                        var project = projects.Find(x => x.Name.Equals(projectName));

                        if (project != null)
                        {
                            TestProperties.SetPropertyValue("CurrentProject", project);
                        }

                        TestMessage += project.ToJson() ?? "Error finding project";
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }


                [TestMethod("Get projects", "This project returns the projects.")]
                public TestVerdict GetProject(
                   [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                   [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                   [TestParameter("Password", "Enter user password", "[Password]")]
                    string password
                   )
                {
                    try
                    {
                        var requestUri = $"{endpoint}/index.php?/api/v2/get_projects";
                        var body = getResponseBody(getRequest(requestUri, userName, password));

                        TestMessage += body;
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                [TestMethod("Get test suites", "Returns project test suites.")]
                public TestVerdict GetTestSuites(
                    [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                    [TestParameter("TestRail project", "Enter TestRail project ID.", "[ProjectID]")]
                    string projectId,
                    [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                    [TestParameter("Password", "Enter user password", "[Password]")]
                    string password
                    )
                {
                    try
                    {
                        var requestUri = $"{endpoint}/index.php?/api/v2/get_suites/{projectId}";
                        var body = getResponseBody(getRequest(requestUri, userName, password));

                        TestMessage += body;
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                [TestMethod("Get test suite test cases", "Returns test suite test cases.")]
                public TestVerdict GetTestCases(
                   [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                   [TestParameter("TestRail project", "Enter TestRail project ID.", "[ProjectID]")]
                    string projectId,
                   [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                   [TestParameter("Password", "Enter user password", "[Password]")]
                    string password,
                   [TestParameter("Suite ID", "Enter the test suite ID ")]
                    string suiteId
                   )
                {
                    try
                    {
                        var requestUri = $"{endpoint}/index.php?/api/v2/get_cases/{projectId}&suite_id={suiteId}";
                        var body = getResponseBody(getRequest(requestUri, userName, password));

                        TestMessage += body;
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                [TestMethod("Get test runs", "Returns project test runs.")]
                public TestVerdict GetTestRuns(
                    [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                    [TestParameter("TestRail project", "Enter TestRail project ID.", "[ProjectID]")]
                    string projectId,
                    [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                    [TestParameter("Password", "Enter user password", "[Password]")]
                    string password
                    )
                {
                    try
                    {
                        var requestUri = $"{endpoint}/index.php?/api/v2/get_runs/{projectId}";
                        var body = getResponseBody(getRequest(requestUri, userName, password));
                        var runs = Run.CreateList(body);

                        //foreach(Run run in runs)
                        //{
                        //    var success = deleteRun(endpoint, "jmothershead@quintity.com", "Invalid1!", run.Id.ToString());
                        //}

                        TestMessage += body;
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                [TestMethod("Get test run", "Returns project test runs.")]
                public TestVerdict GetTestRun(
                    [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                    [TestParameter("TestRail project", "Enter TestRail project ID.", "[ProjectID]")]
                    string projectId,
                    [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                    [TestParameter("Password", "Enter user password", "[Password]")]
                    string password
                    )
                {
                    try
                    {

                        var body = getRun(endpoint, userName, password, _currentRun.Id.ToString());

                        //foreach(Run run in runs)
                        //{
                        //    var success = deleteRun(endpoint, "jmothershead@quintity.com", "Invalid1!", run.Id.ToString());
                        //}

                        TestMessage += body;
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                [TestMethod("Get test case results", "Returns a test cases results.")]
                public TestVerdict GetTestCaseResults(
                    [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                    [TestParameter("TestRail project", "Enter TestRail project ID.", "[ProjectID]")]
                    string projectId,
                    [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                    [TestParameter("Password", "Enter user password", "[Password]")]
                    string password,
                    [TestParameter("Test case ID", "Enter the test case ID")]
                    string caseId
                    )
                {
                    try
                    {
                        var requestUri = $"{endpoint}/index.php?/api/v2/get_results/{caseId}";
                        var body = getResponseBody(getRequest(requestUri, userName, password));

                        var results = Result.CreateList(body);

                        TestMessage += body;
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                [TestMethod("Add test run.", "Adds a project test run.")]
                public TestVerdict AddTestRun(
                   [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                   [TestParameter("TestRail project", "Enter TestRail project ID.", "[ProjectID]")]
                    string projectId,
                   [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                   [TestParameter("Password", "Enter user password", "[Password]")]
                    string password,
                   [TestParameter("Body Request", "Enter body request.")]
                    string bodyRequest
                   )
                {
                    try
                    {
                        var requestUri = $"{endpoint}/index.php?/api/v2/add_run/{projectId}";

                        var body = getResponseBody(postRequest(requestUri, userName, password, bodyRequest));

                        TestMessage += body;
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                private Run _currentRun = null;

                [TestMethod("Add test run.", "Adds a project test run (project is pulled from TestProperties.")]
                public TestVerdict AddTestRun(
                  [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                  [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                  [TestParameter("Password", "Enter user password", "[Password]")]
                    string password,
                  [TestParameter("Body Request", "Enter body request.")]
                    string bodyRequest
                  )
                {
                    try
                    {
                        var project = TestProperties.GetPropertyValue("CurrentProject") as Project;

                        var run = addRun(endpoint, project.Id.ToString(), userName, password, bodyRequest);

                        // Storing run as a class data member.
                        _currentRun = run;

                        TestMessage += run.ToString();
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                [TestMethod("Update existing test run.", "Updates and existing test run (run is pulled from TestProperties.")]
                public TestVerdict UpdateTestRun(
                  [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                  [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                  [TestParameter("Password", "Enter user password", "[Password]")]
                    string password,
                  [TestParameter("Run Id", "Enter test run Id", "[TestRunId]")]
                    int runId,
                  [TestParameter("Body Request", "Enter body request.")]
                    string bodyRequest
                  )
                {
                    try
                    {
                        var project = TestProperties.GetPropertyValue("CurrentProject") as Project;

                        var run = updateRun(endpoint, userName, password, _currentRun.Id.ToString(), bodyRequest);

                        // Storing value on in global TestProperties
                        TestProperties.SetPropertyValue("CurrentRun", run);

                        TestMessage += run.ToString();
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                [TestMethod("Close existing test run.", "Closes an existing test run (run is pulled from TestProperties.")]
                public TestVerdict CloseTestRun(
                  [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                  [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                  [TestParameter("Password", "Enter user password", "[Password]")]
                    string password,
                  [TestParameter("Body Request", "Enter body request.")]
                    string bodyRequest
                  )
                {
                    try
                    {
                        var currentRun = _currentRun;

                        var run = closeRun(endpoint, userName, password, currentRun.Id.ToString(), string.Empty);

                        TestProperties.SetPropertyValue("CurrentRun", run);

                        TestMessage += run.ToString();
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                [TestMethod("Add test result.", "Adds a test result to a case.")]
                public TestVerdict AddTestResult(
                  [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                  [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                  [TestParameter("Password", "Enter user password", "[Password]")]
                    string password,
                  [TestParameter("Test case ID", "Enter the test case Id.")]
                    string testId,
                  [TestParameter("Request file", "Enter path to requestfile.", @"[TestData]\")]
                    string bodyRequestFile
                  )
                {
                    try
                    {
                        var bodyRequest = File.ReadAllText(bodyRequestFile);

                        var requestUri = $"{endpoint}/index.php?/api/v2/add_result/{testId}";

                        var body = getResponseBody(postRequest(requestUri, userName, password, bodyRequest));

                        TestMessage += body;
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                [TestMethod("Add test result for a test case.", "Adds a test result to a case.")]
                public TestVerdict AddTestResultForCase(
                  [TestParameter("TestRail Uri", "Enter TestRail Uri", "[Endpoint]")]
                    string endpoint,
                  [TestParameter("User name", "Enter TestRail user name.", "[UserName]")]
                    string userName,
                  [TestParameter("Password", "Enter user password", "[Password]")]
                    string password,
                  [TestParameter("Test case ID", "Enter the test case Id.")]
                    string caseId,
                  [TestParameter("Request file", "Enter path to requestfile.", @"[TestData]\")]
                    string bodyRequestFile
                  )
                {
                    try
                    {
                        var bodyRequest = File.ReadAllText(bodyRequestFile);

                        var result = addResultForCase(endpoint, userName, password, _currentRun.Id.ToString(), caseId, bodyRequest);

                        TestMessage += result.ToJson();
                        TestVerdict = TestVerdict.Pass;
                    }
                    catch (Exception e)
                    {
                        TestMessage += e.Message;
                        TestVerdict = TestVerdict.Error;
                    }
                    finally
                    {
                        Teardown();
                    }

                    return TestVerdict;
                }

                #endregion

                #region Private members

                private bool deleteRun(string endpoint, string userName, string password, string runId)
                {
                    var requestUri = $"{endpoint}/index.php?/api/v2/delete_run/{runId}";

                    var response = postRequest(requestUri, userName, password, string.Empty);

                    return response.StatusCode == HttpStatusCode.OK ? true : false;
                }

                private Run addRun(string endpoint, string projectId, string userName, string password, string bodyRequest)
                {
                    var requestUri = $"{endpoint}/index.php?/api/v2/add_run/{projectId}";

                    var body = getResponseBody(postRequest(requestUri, userName, password, bodyRequest));

                    return Run.Create(body);
                }

                private Run updateRun(string endpoint, string userName, string password, string runId, string bodyRequest)
                {
                    var requestUri = $"{endpoint}/index.php?/api/v2/update_run/{runId}";

                    var body = getResponseBody(postRequest(requestUri, userName, password, bodyRequest));

                    return Run.Create(body);
                }

                private Run getRun(string endpoint, string userName, string password, string runId)
                {
                    var requestUri = $"{endpoint}/index.php?/api/v2/get_run/{runId}";

                    var body = getResponseBody(getRequest(requestUri, userName, password));

                    return Run.Create(body);
                }

                private Result addResultForCase(string endpoint, string userName, string password, string runId, string caseId, string bodyRequest)
                {
                    var requestUri = $"{endpoint}/index.php?/api/v2/add_result_for_case/{runId}/{caseId}";

                    var body = getResponseBody(postRequest(requestUri, userName, password, bodyRequest));

                    return Result.Create(body);
                }

                private Run closeRun(string endpoint, string userName, string password, string runId, string bodyRequest)
                {
                    var requestUri = $"{endpoint}/index.php?/api/v2/close_run/{runId}";

                    var body = getResponseBody(postRequest(requestUri, userName, password, bodyRequest));

                    return Run.Create(body);
                }

                private List<Project> getProjects(string endpoint, string userName, string password)
                {
                    var requestUri = $"{endpoint}/index.php?/api/v2/get_projects";
                    var body = getResponseBody(getRequest(requestUri, userName, password));

                    return Project.CreateList(body);
                }

                private string getResponseBody(HttpWebResponse webResponse)
                {
                    // Get the stream associated with the response.
                    Stream responseStream = webResponse.GetResponseStream();
                    StreamReader readStream = new StreamReader(responseStream);

                    return readStream.ReadToEnd();
                }

                private HttpWebResponse getRequest(string requestUri, string userName, string password)
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    // Create web request for json query.
                    WebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                    request.Method = WebRequestMethods.Http.Get;
                    request.ContentType = "application/json";

                    string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));

                    // Set addition header content
                    request.Headers.Add("Authorization", "Basic " + auth);

                    // Make request/get response
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    return response;
                }

                private HttpWebResponse postRequest(string requestUri, string userName, string password, string bodyRequest)
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                    // Create web request for json query.
                    WebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                    request.Method = WebRequestMethods.Http.Post;
                    request.ContentType = "application/json";
                    request.ContentLength = bodyRequest.Length;
                    request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                    // Add basic authentication to request header.
                    string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));
                    request.Headers.Add("Authorization", "Basic " + auth);

                    // Write query string to request.
                    var requestStream = request.GetRequestStream();
                    StreamWriter postStream = new StreamWriter(requestStream, System.Text.Encoding.ASCII);
                    postStream.Write(bodyRequest);
                    postStream.Close();

                    // Make request/get response
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    return response;
                }

                #endregion
        */
    }
}

