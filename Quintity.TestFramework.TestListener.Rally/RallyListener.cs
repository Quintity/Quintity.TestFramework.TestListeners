using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quintity.TestFramework.TestListener;

namespace Quintity.TestFramework.TestListener.Rally
{
    public class RallyListener : Quintity.TestFramework.Core.TestListener
    {
        public override void OnTestCaseExecutionBegin(Core.TestCase testCase)
        {
            throw new NotImplementedException();
        }

        public override void OnTestCaseExecutionComplete(Core.TestCase testCase, Core.TestCaseResult testCaseResult)
        {
            throw new NotImplementedException();
        }

        public override void OnTestCheck(Core.TestCheck testCheck)
        {
            throw new NotImplementedException();
        }

        public override void OnTestExecutionBegin(Core.TestExecutor testExecutor)
        {
            throw new NotImplementedException();
        }

        public override void OnTestExecutionComplete(Core.TestExecutor testExecutor, Core.TestExecutionCompleteArgs args)
        {
            throw new NotImplementedException();
        }

        public override void OnTestPostprocessorBegin(Core.TestSuite testSuite)
        {
            throw new NotImplementedException();
        }

        public override void OnTestPostprocessorComplete(Core.TestSuite testSuite, Core.TestProcessorResult testProcessorResult)
        {
            throw new NotImplementedException();
        }

        public override void OnTestPreprocessorBegin(Core.TestSuite testSuite)
        {
            throw new NotImplementedException();
        }

        public override void OnTestPreprocessorComplete(Core.TestSuite testSuite, Core.TestProcessorResult testProcessorResult)
        {
            throw new NotImplementedException();
        }

        public override void OnTestStepExecutionBegin(Core.TestStep testStep)
        {
            throw new NotImplementedException();
        }

        public override void OnTestStepExecutionComplete(Core.TestStep testStep, Core.TestStepResult testStepResult)
        {
            throw new NotImplementedException();
        }

        public override void OnTestSuiteExecutionBegin(Core.TestSuite testSuite)
        {
            throw new NotImplementedException();
        }

        public override void OnTestSuiteExecutionComplete(Core.TestSuite testSuite, Core.TestSuiteResult testSuiteResult)
        {
            throw new NotImplementedException();
        }

        public override void OnTestTrace(string traceMessage)
        {
            throw new NotImplementedException();
        }

        public override void OnTestWarning(Core.TestWarning testWarning)
        {
            throw new NotImplementedException();
        }
    }
}
