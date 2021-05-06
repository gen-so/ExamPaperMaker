using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ExamPaperMakerDebugger
{
    class Tools
    {

        //Creates a new blank A4 page
        public static Bitmap CreateNewPage()
        {
            //create image at A4 size, H:3508 x W:2480 px
            var bmpOut = new Bitmap(Program.A4Width,Program.A4Height);

            using var g = Graphics.FromImage(bmpOut);
            g.Clear(Color.White);

            return bmpOut;
        }

        public static List<Bitmap> GetQuestion()
        {
            //get a list of all questions available
            var questionNames = Directory.GetFiles("questions/", "*.jpg");

            //get the file of each question and put it in a list
            var questionList = new List<Bitmap>();
            foreach (var name in questionNames)
            {
                questionList.Add(new Bitmap(name));
            }

            //return list to caller
            return questionList;
        }
    }
}
