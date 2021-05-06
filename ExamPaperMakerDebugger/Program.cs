using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;


namespace ExamPaperMakerDebugger
{
    class Program
    {
        //Fixed measurements for A4 in px
        public const int A4Width = 2480; //X
        public const int A4Height = 3508; //Y

        //Page Margins
        public const int LeftMargin = 200;
        public const int RightMargin = A4Width - LeftMargin; //equal sides
        public const int TopMargin = 250;
        public const int BottomMargin = A4Height - 200;//equal sides
        public const int PageCenter = A4Width / 2; //where the line is drawn
        public const int Column2LeftMargin = PageCenter + LeftMargin; //X


        //the top bottom space between question (Y axis)
        public const int QuestionPadding = 80; 

        //public const int A4Width2 = 595;
        //public const int A4Height2 = 841;
        //public const int Column1VerticalMargin2 = 66;
        //public const int Column2VerticalMargin2 = 333;


        //the starting point where questions will be placed in the column
        private static readonly Point Column1Start = new(LeftMargin, TopMargin);
        private static readonly Point Column2Start = new(Column2LeftMargin, TopMargin);

        public static List<Bitmap> PageList = new(); 

        public static readonly SolidBrush Black = new(Color.Black);

        static void Main(string[] args)
        {

            //create a blank A4 page
            var pg1 = Tools.CreateNewPage();

            //get all questions
            var questionList = Tools.GetQuestion();

            var addedQuestionCount = 0;

            //keep track of the place to put the question accordingly
            //first question goes to start of column 1
            var placement = new QuestionLocation { Page = 1, Point = Column1Start };
            var tryAddCount = 0;
            var skippedBefore = false;
            //place the questions into the page
            //as long as there are questions in the list
            while (questionList.Count > 0)
            {
                //make a copy of the questions for looping only
                //since the main list will be modified as questions gets removed once done
                var loopList = new List<Bitmap>(questionList);
                
                foreach (var question in loopList)
                {
                    //check if question can be added into current location comfortably
                    var isValid = IsLocationValidForQuestion(question, placement);
                    
                    //if can fit, place the question at that location
                    if (isValid)
                    {
                        //get the page
                        var page = GetPage(placement.Page);
                        using var editablePage = Graphics.FromImage(page);
                        //place put the question inside
                        editablePage.DrawImage(question, placement.Point);
                        
                       //add question number
                        var fnt = new Font("Verdana", 37, GraphicsUnit.Pixel);
                        addedQuestionCount++; //use for numbering the questions
                        editablePage.DrawString(addedQuestionCount.ToString(), fnt,
                                                Black,
                                                placement.Point.X - 50, //move a little to the left
                                                placement.Point.Y+ 2);
                        

                        //remove the question from the main list
                        questionList.Remove(question);

                        //update the placement for the next question
                        placement = GetNextPlacement(placement, question);

                        skippedBefore = false;
                    }
                    else
                    {
                        skippedBefore = true;

                    }
                }

                //if still got unadded questions, but loop has finished
                //then current placement is too small for anybody to fit
                //manually move placement to the next column or next page
                if (questionList.Count > 0 && skippedBefore) { placement = ForceMovePlacement(placement); }

                //increase try count
                tryAddCount++;
                Console.WriteLine($"Try count:{tryAddCount}\tQuestions left:{questionList.Count}");
            }


            //save each page to file in output
            var pageCount = 1;
            foreach (var page in PageList)
            {
                //add lines to page
                AddCenterLineToPage(page);
                AddHeaderLineToPage(page);

                //add page number
                AddPageNumber(page, pageCount);

                page.Save($"output/{pageCount}.jpg");
                pageCount++;
            }


            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void AddPageNumber(Bitmap page, int pageNumber)
        {
            using var editablePage = Graphics.FromImage(page);
            var fnt = new Font("Verdana", 45,FontStyle.Bold, GraphicsUnit.Pixel );
            
            editablePage.DrawString(pageNumber.ToString(), fnt,
                Black,
                PageCenter - 20,
                (BottomMargin + 30));
        }
        
        private static void AddCenterLineToPage(Bitmap page)
        {

            using var editablePage = Graphics.FromImage(page);

            Pen pn = new Pen(Color.Black);
            pn.Width = 3;

            Point pt1 = new Point(PageCenter, TopMargin);
            Point pt2 = new Point(PageCenter, BottomMargin -10);

            editablePage.DrawLine(pn, pt1, pt2);

        }
        
        private static void AddHeaderLineToPage(Bitmap page)
        {

            using var editablePage = Graphics.FromImage(page);

            Pen pn = new Pen(Color.Black);
            pn.Width = 3;

            Point pt1 = new Point(LeftMargin, TopMargin - 55);
            Point pt2 = new Point(RightMargin, TopMargin - 55);

            editablePage.DrawLine(pn, pt1, pt2);

        }

        //Manually moves the placement to next page or column because no question in list can fit
        private static QuestionLocation ForceMovePlacement(QuestionLocation placement)
        {
            //find out which column the point falls in (also checks left right margins)
            var column = GetColumnNumber(placement.Point);

            Point newPoint;
            var newPage = placement.Page;
            
            switch (column)
            {
                //move to 2nd column if this is the 1st
                case 1: newPoint = Column2Start; break;
                //move to next page if already 2nd column
                case 2: { newPage++; newPoint = Column1Start; break; }
                default: throw new Exception("Column number not accounted for!");
            }

            //return new location to caller
            return new QuestionLocation { Page = newPage, Point = newPoint };

        }

        //checks if a question can be added to a page at the specified location
        private static bool IsLocationValidForQuestion(Bitmap question, QuestionLocation location)
        {
            //check if questions height is ok
            var questionBottomY = location.Point.Y + question.Height;
            if (questionBottomY >= BottomMargin) { return false;}

            //check if questions width is ok
            var questionRightX = location.Point.X + question.Width;
            var column = GetColumnNumber(location.Point);

            //based on column, check against different right margins
            return column switch
            {
                1 => questionRightX <= Column2LeftMargin,
                2 => questionRightX <= RightMargin,
                _ => throw new Exception("Column number not accounted for!")
            };
        }

        private static Bitmap GetPage(int page)
        {
            //if page exist in list, return it to caller
            if (PageList.Count >= page) { return PageList[page-1];}
            else
            {
                //make new page, add it to main list
                var newPage = Tools.CreateNewPage();
                PageList.Add(newPage);
                //then return that to caller
                return newPage;
            }
        }

        //gets the point for the next question below current question or in next column
        //keeps the questions from overlapping and in side the page (A4)
        private static QuestionLocation GetNextPlacement(QuestionLocation currentPlacement, Image question)
        {
            //calculate the new points
            var newY = currentPlacement.Point.Y + question.Height + QuestionPadding;
            var newX = currentPlacement.Point.X;
            var newPage = currentPlacement.Page; //assign current page first
            var newPoint = new Point(newX, newY);

            //check if new point is valid, within the margins
            //check top margin
            if (newPoint.Y < TopMargin) { throw new Exception("Point above top margin!"); }

            //find out which column the point falls in (also checks left right margins)
            var column = GetColumnNumber(newPoint);

            //if point below bottom margin, there's no place to add question
            if (newPoint.Y >= BottomMargin)
            {
                switch (column)
                {
                    //move to 2nd column if this is the 1st
                    case 1: newPoint = Column2Start; break;
                    //move to next page if already 2nd column
                    case 2:{ newPage++; newPoint = Column1Start; break;}
                    default: throw new Exception("Column number not accounted for!");
                }
            }

            //return new location to caller
            return new QuestionLocation{Page = newPage, Point = newPoint};
        }

        //determines the column the point falls into
        //also checks if point is within left & right margin, will throw exception otherwise
        private static int GetColumnNumber(Point point)
        {
            //if before 2nd column's horizontal margin then 1st column
            if (point.X < Column2LeftMargin)
            {
                //in column 1, make sure it is within left margin
                if (point.X < LeftMargin) { throw new Exception("Point before left margin!");}

                return 1;
            }
            else
            {
                //in column 2, make sure it is within right margin
                if (point.X > RightMargin) { throw new Exception("Point after right margin!"); }

                return 2;
            }
        }
    }

}

