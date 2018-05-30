using System;
using System.Net.Mail;
using Quintity.TestFramework.Core;
using Quintity.TestFramework.Runtime;

namespace QuintityTestFramework.TestListeners
{
    public class SMTPListener : TestListener
    {
        #region Data members

        readonly private string[] _padding = 
        { 
            "",
            "|",
            "    |",
            "        |",
            "            |",
            "                |",
            "                    |",
            "                        |",
            "                            |",
            "                                |",
            "                                    |"
        };

        string m_separator = "----------------------------------------------------------------";

        private int _level;

        const int _maxTestCaseTitle = 79;
        const int _maxTestSuiteTitle = _maxTestCaseTitle - 11;
        const string _trailer = ". . .";

        private string _mailHost;
        private int _port;
        private string _sender;
        private string[] _recipients;

        private string _mailSubject;
        private string _mailBody;

        private string _build;
        private string _buildTrigger;

        private decimal _totalPassed;
        private decimal _totalFailed;
        private decimal _totalErrored;
        private decimal _totalInactive;

        #endregion

        #region Listener event handlers

        public override void OnTestExecutionBegin(TestExecutor testExecutor, TestExecutionBeginArgs args)
        {
            _totalPassed = 0;
            _totalFailed = 0;
            _totalErrored = 0;
            _totalInactive = 0;

            string delimitedRecipients;

            _build = Convert.ToString(TestProperties.GetPropertyValue("Build"));
            _buildTrigger = Convert.ToString(TestProperties.GetPropertyValue("BuildTrigger"));
            _mailHost = Convert.ToString(TestProperties.GetPropertyValue("MailHost"));
            _port = Convert.ToInt32(TestProperties.GetPropertyValue("Port"));
            _sender = Convert.ToString(TestProperties.GetPropertyValue("Sender"));

            delimitedRecipients = Convert.ToString(TestProperties.GetPropertyValue("Recipients"));

            _recipients = delimitedRecipients.Split(new char[] { ';' });

            for (int i = 0; i < _recipients.Length; i++)
            {
                _recipients[i] = _recipients[i].Trim();
            }

            // Add computer information.
            appendToBody(string.Format("\r\nTester:  {0}\r\nHost machine:  {1}\r\nRun time:  {2}",
                Environment.UserName, Environment.MachineName, DateTime.Now.ToString("F")));
        }

        public override void OnTestCaseExecutionBegin(TestCase testCase, TestCaseBeginExecutionArgs args) { }

        public override void OnTestCaseExecutionComplete(TestCase testCase, TestCaseResult testCaseResult)
        {
            string title = fixupTitle(testCase.Title, _maxTestCaseTitle);

            appendToBody(string.Format("{0,-6} - {1} - {2,4}", testCase.UserID, title, testCaseResult.TestVerdict));

            switch (testCaseResult.TestVerdict)
            {
                case TestVerdict.Pass:
                    _totalPassed++;
                    break;
                case TestVerdict.Fail:
                    _totalFailed++;
                    break;
                case TestVerdict.Error:
                    _totalErrored++;
                    break;
                case TestVerdict.DidNotExecute:
                    _totalInactive++;
                    break;
                default:
                    break;
            }
        }

        public override void OnTestCheck(TestCheck testCheck) { }

        public override void OnTestExecutionComplete(TestExecutor testExecutor, TestExecutionCompleteArgs args)
        {
            // Prepare and add test run summary to mail body.
            string format = "Test case summary:  Passed: {0} ({1:p1}), Failed:  {2} ({3:p1}), " +
            "Errored:  {4} ({5:P1}), Inactive:  {6} ({7:p1}),  Total:  {8} (100%)";

            decimal total = _totalPassed + _totalFailed + _totalErrored + _totalInactive;

            string summary = string.Format(format,
                _totalPassed,
                total != 0 ? _totalPassed / total : 0,
                _totalFailed,
                total != 0 ? _totalFailed / total : 0,
                _totalErrored,
                total != 0 ? _totalErrored / total : 0,
                _totalInactive,
                total != 0 ? _totalInactive / total : 0,
                total);

            appendToBody(summary);

            // Prepare and send summary email.
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(_sender);

            // Add senders to collection.
            foreach (string recipient in _recipients)
            {
                mail.To.Add(recipient);
            }

            // Prepare subject line and body.
            mail.Subject = _mailSubject;
            mail.Body = _mailBody;

            // Send it.
            SmtpClient client = new SmtpClient(_mailHost, _port);
            client.Send(mail);

            // Cleaning (probably unnecessary).
            _mailHost = null;
            _sender = null;
            _recipients = null;
            _build = null;
            _buildTrigger = null;
        }

        public override void OnTestPostprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args) { }

        public override void OnTestPostprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        {
            appendToBody(string.Format("Post-process::  {0}, Message:  {1}", testProcessorResult.TestVerdict,
                testProcessorResult.TestMessage == null ? "None" : testProcessorResult.TestMessage));
        }

        public override void OnTestPreprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args) { }

        public override void OnTestPreprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        {
            appendToBody(string.Format("Pre-process:  {0}, Message:  {1}", testProcessorResult.TestVerdict,
                 testProcessorResult.TestMessage == null ? "None" : testProcessorResult.TestMessage));
        }

        public override void OnTestStepExecutionBegin(TestStep testStep, TestStepBeginExecutionArgs args) { }

        public override void OnTestStepExecutionComplete(TestStep testStep, TestStepResult testStepResult) { }

        public override void OnTestSuiteExecutionBegin(TestSuite testSuite, TestSuiteBeginExecutionArgs args)
        {
            _level++;

            this._mailBody += "\r\n";

            var guid = Guid.NewGuid();

            if (testSuite.ParentID == null)
            {
                _mailSubject = string.Format("Build {0} ({1}) - {2} - {3}", _buildTrigger, _build, testSuite.Title, "{0}");
            }

            string title = testSuite.Title;

            if (title.Length > _maxTestSuiteTitle)
            {
                title = title.Substring(0, _maxTestSuiteTitle - _trailer.Length) + _trailer;
            }

            appendToBody(string.Format("Test suite {0, -6}:  {1} - {2}", testSuite.UserID, title, "{0}"), ">");

            if (testSuite.TestPreprocessor.Status != Status.Active)
            {
                appendToBody("Pre-processor:  Did not execute.");
                appendToBody(m_separator);
            }
        }

        public override void OnTestSuiteExecutionComplete(TestSuite testSuite, TestSuiteResult testSuiteResult)
        {
            // If topmost test suite, prepare subject line and body.
            if (testSuite.ParentID == null)
            {
                _mailSubject = string.Format(_mailSubject, testSuiteResult.TestVerdict);
                _mailBody += "\r\n";
            }

            _mailBody = string.Format(_mailBody, testSuiteResult.TestVerdict);

            appendToBody(m_separator);

            if (testSuite.TestPreprocessor.Status != Status.Active)
            {
                appendToBody("Post-processor:  Did not execute.");
            }

            appendToBody(string.Format("Suite Summary:  Passed:  {0}, Failed:  {1}, Errors:  {2}, Inactive:  {3}, Total:  {4}",
                testSuiteResult.Passed, testSuiteResult.Failed, testSuiteResult.Errored, testSuiteResult.DidNotExecute, testSuiteResult.Total));

            appendToBody(string.Format("Start Time: {0}, End Time:  {1}, Duration:  {2}",
                testSuiteResult.StartTime.ToString("T", null), testSuiteResult.EndTime.ToString("T", null), testSuiteResult.ElapsedTime.ToString(@"dd\.hh\:mm\:ss")), ">");

            _mailBody += "\r\n";

            _level--;
        }

        public override void OnTestTrace(string virtualString, string traceMessage) { }

        public override void OnTestWarning(TestWarning testWarning) { }

        public override void OnTestMetric(string virtualUser, TestMetricEventArgs args) { }

        #endregion

        #region Private methods

        private void appendToBody(string text)
        {
            appendToBody(text, null);
        }

        private void appendToBody(string text, string characters)
        {
            string formatedtext = getFormatedBodyText(text);

            if (characters != null)
            {
                formatedtext = formatedtext.Replace("|", characters);
            }

            _mailBody += formatedtext;
        }

        private string getFormatedBodyText(string text)
        {
            return string.Format("{0}{1}\r\n", _padding[_level], text);
        }

        private void appendCRToBody()
        {
            _mailBody += string.Format("\r\n{0}", _padding[_level]);
        }

        private string fixupTitle(string title, int maxlength)
        {
            string fixedUpTitle = null;

            if (title.Length > maxlength)
            {
                fixedUpTitle = title.Substring(0, maxlength - _trailer.Length) + _trailer;
            }
            else
            {
                fixedUpTitle = title.PadRight(maxlength);
            }

            return fixedUpTitle;
        }

        #endregion

    }
}
