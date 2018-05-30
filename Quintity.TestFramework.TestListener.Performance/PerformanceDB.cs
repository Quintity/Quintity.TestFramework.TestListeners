using System;
using System.Diagnostics;
using System.Data.SqlClient;
using Quintity.TestFramework.Core;
using Quintity.TestFramework.Runtime;

namespace Quintity.TestFramework.TestListener.Performance
{
    public class PerformanceDB : Quintity.TestFramework.Runtime.TestListener
    {
        #region Data members

        static private SqlConnection sqlConnection = null;

        private int testRunId = -1;

        private readonly string sqlConnectionString = @"Server=localhost;Database=Performance;Trusted_Connection=True;";
        private readonly string testRunInsert = "INSERT INTO [Performance].[dbo].[TestRun] ([StartDate]) VALUES ('{0}'); SELECT SCOPE_IDENTITY();";
        private readonly string testMetricInsert =
            "INSERT INTO [dbo].[TestMetrics]" +
               " ([TestRunId]" +
               ", [VirtualUser]" +
               ", [PerfId]" +
               ", [Description]" +
               ", [StartTime]" +
               ", [EndTime]" +
               ", [ElapsedTime]" +
               ", [StateArgs]) " +
            "VALUES " +
               "( '{0}'" +
               ", '{1}'" +
               ", '{2}'" +
               ", '{3}'" +
               ", '{4}'" +
               ", '{5}'" +
               ",  {6}" +
               ", '{7}')";

        private readonly string testResultInsert =
            "INSERT INTO [dbo].[TestResults]" +
               " ([TestRunId]" +
               ", [VirtualUser]" +
               ", [StartTime]" +
               ", [EndTime]" +
               ", [ElapsedTime]" +
               ", [TestVerdict]) " +
            "VALUES " +
               "( '{0}'" +
               ", '{1}'" +
               ", '{2}'" +
               ", '{3}'" +
               ", '{4}'" +
               ", '{5}')";

        #endregion

        public override void OnTestExecutionBegin(TestExecutor testExecutor, TestExecutionBeginArgs args)
        {
            try
            {
                if (sqlConnection == null)
                {
                    sqlConnection = new SqlConnection(sqlConnectionString);

                    using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                    {
                        if (sqlConnection.State != System.Data.ConnectionState.Open)
                        {
                            sqlConnection.Open();
                        }

                        sqlCommand.CommandText = string.Format(testRunInsert, DateTime.Now);
                        testRunId = Convert.ToInt32(sqlCommand.ExecuteScalar());
                    }
                }
            }
            catch (Exception e)
            {
                TestTrace.Trace(e.ToString());
            }
        }

        public override void OnTestExecutionComplete(TestExecutor testExecutor, TestExecutionCompleteArgs args)
        {
            if (sqlConnection.State != System.Data.ConnectionState.Closed)
            {
                sqlConnection.Close();
            }
        }

        public override void OnTestMetric(string virtualUser, TestMetricEventArgs args)
        {
            try
            {
                if (!args.ElapsedTime.Equals(TimeSpan.Zero))
                {
                    var flattenedArgs = args.FlattenStateArgs();

                    var metric = string.Format(testMetricInsert,
                        testRunId,
                        args.VirtualUser,
                        args.PerfID,
                        args.Description,
                        args.StartTime,
                        args.StopTime,
                        args.ElapsedTime.TotalSeconds,
                        flattenedArgs);
#if VERBOSE
                    TestTrace.Trace(metric);
#endif

                    using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                    {
                        sqlCommand.CommandText = metric;
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                TestTrace.Trace(e.ToString());
            }
        }

        public override void OnTestSuiteExecutionComplete(TestSuite testSuite, TestSuiteResult testSuiteResult)
        {
            try
            {
                var metric = string.Format(testResultInsert,
                        testRunId,
                        testSuiteResult.VirtualUser,
                        testSuiteResult.StartTime,
                        testSuiteResult.EndTime,
                        testSuiteResult.ElapsedTime,
                        testSuiteResult.TestVerdict);

                Debug.WriteLine(metric);

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = metric;
                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                TestTrace.Trace(e.ToString());
            }
        }

        public override void OnTestCaseExecutionBegin(TestCase testCase, TestCaseBeginExecutionArgs args)
        { }

        public override void OnTestCaseExecutionComplete(TestCase testCase, TestCaseResult testCaseResult)
        { }

        public override void OnTestCheck(TestCheck testCheck)
        { }

        public override void OnTestPostprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args)
        { }

        public override void OnTestPostprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        { }

        public override void OnTestPreprocessorBegin(TestSuite testSuite, TestProcessorBeginExecutionArgs args)
        { }

        public override void OnTestPreprocessorComplete(TestSuite testSuite, TestProcessorResult testProcessorResult)
        { }

        public override void OnTestStepExecutionBegin(TestStep testStep, TestStepBeginExecutionArgs args)
        { }

        public override void OnTestStepExecutionComplete(TestStep testStep, TestStepResult testStepResult)
        { }

        public override void OnTestSuiteExecutionBegin(TestSuite testSuite, TestSuiteBeginExecutionArgs args)
        { }

        public override void OnTestTrace(string virtualUser, string traceMessage)
        { }

        public override void OnTestWarning(TestWarning testWarning)
        { }
    }
}
