using System.Drawing;

namespace ExamPaperMakerDebugger
{
    /// <summary>
    /// Simple data class to hold the location of
    /// a question in the exam sheet and page
    /// </summary>
    public class QuestionLocation
    {
        public int Page; //location in exam sheet
        public Point Point; //location in page
    }
}