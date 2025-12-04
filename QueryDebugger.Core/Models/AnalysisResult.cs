using System.Collections.Generic;

namespace QueryDebugger.Core.Models
{
    public class AnalysisResult
    {
        public bool IsSuccess { get; set; }
        public List<StepResult> Steps { get; set; } = new List<StepResult>();
    }

    public class StepResult
    {
        public string StepName { get; set; }
        public string Description { get; set; }
        public bool Passed { get; set; }
        public string ErrorMessage { get; set; }
        public string GeneratedQuery { get; set; }
        public long RecordCount { get; set; }
    }
}

