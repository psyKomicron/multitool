using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace MultitoolWPF
{
    internal class ExceptionTextFormatter
    {
        private static CorrespondanceTable[] Table = new CorrespondanceTable[]
        {
            new CorrespondanceTable(typeof(ArgumentException), Colors.Red),
            new CorrespondanceTable(typeof(OperationCanceledException), Colors.Yellow),
            new CorrespondanceTable(typeof(Exception), Colors.Red)
        };

        public List<Run> GetFormatting(Exception e)
        {
            List<Run> results = new List<Run>(10)
            {
                new Run() 
                { 
                    Foreground = new SolidColorBrush(GetExceptionColor(e)), 
                    Text = "\n" +  e.GetType().Name + " thrown in " + (e.Source == string.Empty ? "unknow" : e.Source)
                },
                new Run()
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    Text = "\n" + e.Message
                },new Run()
                {
                    Foreground = new SolidColorBrush(Colors.Green),
                    Text = "\n\tHRESULT : " + e.HResult
                }
            };

            if (e.Data.Count > 0)
            {
                string data = "\n\tException data :";
                foreach (KeyValuePair<object, string> tuple in e.Data)
                {
                    if (tuple.Key.GetType() == typeof(Type))
                    {
                        // normalised exception
                        data += "\n\t\t⊢ Exception : " + tuple.Key.ToString();
                        data += "\n\t\t\t∟ Message : " + tuple.Value;
                    }
                }
                data += "\n\t∟ Data end.\n";

                results.Add(new Run() { Foreground = new SolidColorBrush(Colors.White), Text = data });
            }
            else
            {
                results.Add(new Run() { Foreground = new SolidColorBrush(Colors.YellowGreen), Text = "\n\tNo data for this exception.\n" });
            } 

            return results;
        }

        private Color GetExceptionColor(Exception e)
        {
            for (int i = 0; i < Table.Length; i++)
            {
                if (Table[i].ExceptionType.IsAssignableFrom(e.GetType()))
                {
                    return Table[i].ExceptionColor;
                }
            }
            return Colors.White;
        }
    }

    internal class Result
    {
        public Color Color { get; set; }
        public string Message { get; set; }
    }

    struct CorrespondanceTable
    {
        public Type ExceptionType;
        public Color ExceptionColor;

        public CorrespondanceTable(Type t, Color c)
        {
            ExceptionType = t;
            ExceptionColor = c;
        }
    }
}
