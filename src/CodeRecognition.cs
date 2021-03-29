#define debug
using System;
using System.Globalization;
using System.Speech.Recognition;
using System.Threading;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;
using System.Text;
using System.Xml;

namespace Voice_Coding.src
{
    class CodeRecognition
    {
        #region Var declaration
        public  bool                                recognizing;
        private int                                 level = 0;
        public  event    EventHandler               ExitEvent;
        private readonly SpeechRecognitionEngine    rec;
        private readonly InputSimulator             sim;
        private readonly StatusBar                  statusBar;
        private readonly string[] cmd;
        private readonly string[] opr;
        private readonly string[] aph;
        private readonly string[] num;
        #endregion


        public CodeRecognition()
        {
            //load xml file and create obj doc
            XmlDocument doc = new XmlDocument();

#if debug
            doc.Load(@"D:\MY PROJECTS\8th sem project\Voice-Coding-EditChanges\res\MainResource.xml");
#else
            doc.Load(@"Resources\MainResource.xml");
#endif

            //create nodelist obj and get it by id command
            XmlNodeList nodeList = doc.GetElementsByTagName("Command");

            //add value in  cmd[]
            cmd = new string[nodeList.Count];
            int i = 0;
            foreach (XmlNode node in nodeList)
            {
                cmd[i] = node.Attributes["value"]?.InnerText;
                i++;
            }

            //add same for datatype attribute in mainresource.xml
          /*  XmlNodeList nodeList1 = doc.GetElementsByTagName("Datatype");
            string[] datatype = new string[nodeList1.Count];
            int j = 0;
            foreach (XmlNode node1 in nodeList1)
            {   
                
                datatype[j] = node1.Attributes["name"]?.InnerText;
                j++;
            }
          */
            //add same for operator tag
            XmlNodeList nodeList3 = doc.GetElementsByTagName("Operator");
            opr = new string[nodeList3.Count];
            int m = 0;
            foreach (XmlNode node3 in nodeList3)
            {
                opr[m] = node3.Attributes["name"].Value;
                m++;
            }

            //add same for alphabet
            XmlNodeList nodeList5 = doc.GetElementsByTagName("Alphabet");
            aph = new string[nodeList5.Count];
            int x = 0;
            foreach (XmlNode node5 in nodeList5)
            {
                aph[x] = node5.Attributes["name"].Value;
                x++;
            }


            //add same for number
            XmlNodeList nodeList6 = doc.GetElementsByTagName("Number");
            num = new string[nodeList6.Count];
            int y = 0;
            foreach (XmlNode node6 in nodeList6)
            {
                num[y] = node6.Attributes["name"].Value;
                y++;
            }

            rec = new SpeechRecognitionEngine(new CultureInfo("en-US"));
            rec.SetInputToDefaultAudioDevice();
            
            //load grammer and build grammer so we directly use and recognise by swich case
            rec.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(cmd))));
            rec.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(aph))));
            rec.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(num))));
            rec.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(opr))));

            rec.LoadGrammarAsync(new CPPGrammar().GetGrammar);


            //All event handler
            rec.SpeechRecognized +=
                new EventHandler<SpeechRecognizedEventArgs>   (Rec_Recognised);
            rec.SpeechDetected +=
                new EventHandler<SpeechDetectedEventArgs>     (Rec_Detected);
            rec.RecognizeCompleted +=
                new EventHandler<RecognizeCompletedEventArgs> (Rec_Completed);
            rec.AudioLevelUpdated +=
                new EventHandler<AudioLevelUpdatedEventArgs>  (Rec_AudioUpdate);

            sim = new InputSimulator();

            statusBar                    = new StatusBar();
            statusBar.Show();
            statusBar.ToggleRecogniton  += new EventHandler<RoutedEventArgs>(OnToggle);
            statusBar.Exit              += new EventHandler<RoutedEventArgs>(OnExitEvent);
        }

        private void Rec_Recognised(object sender, SpeechRecognizedEventArgs e)
        {
            string[] words = e.Result.Text.Split(' ');
            string rslt="<NOT SETED>", data = "<NOT SETED>";

            Console.WriteLine("CMD: " + words[0]);
            
            if(words.Length > 1)
            {
                rslt = FindInDictionary(words[1]);
                data = e.Result.Text.Replace(words[0] + " " + words[1] + " ", "");
            }

            statusBar.ChangeText($"{e.Result.Text} [{e.Result.Confidence}] [{e.Result.Grammar.Name}]");

            //for direct use of command
            XmlDocument doc = new XmlDocument();

#if debug
            doc.Load(@"D:\MY PROJECTS\8th sem project\Voice-Coding-EditChanges\res\MainResource.xml");
#else
            doc.Load(@"Resources\MainResource.xml");
#endif


            XmlNodeList nodeList4 = doc.GetElementsByTagName("Operator"); // obj for operator
            string[] opt1 = new string[nodeList4.Count];
            int n = 0;
            //for operator recognization
            foreach (XmlNode node4 in nodeList4)
            {
                opt1[n] = node4.Attributes["name"]?.InnerText;
                n++;
            }
            for (int o = 0; o < opt1.Length; o++)
            {

                if (words[0] == opt1[o].ToString())
                {   

                    sim.Keyboard.TextEntry($"{FindInDictionary(words[0])}" +" ");
                    break;
                }
            }

            //for alphabet 
            XmlNodeList nodeList7 = doc.GetElementsByTagName("Alphabet");
            string[] aph1 = new string[nodeList7.Count];
            int c = 0;
            foreach (XmlNode node7 in nodeList7)
            {
                aph1[c] = node7.Attributes["name"]?.InnerText;
                c++;
            }
            for ( c = 0; c < aph1.Length; c++)
            {

                if (words[0] == aph1[c].ToString())
                {

                    sim.Keyboard.TextEntry($"{FindInDictionary(words[0])}" + " ");
                    break;
                }
            }

            //for number
            XmlNodeList nodeList8 = doc.GetElementsByTagName("Number");
            string[] num1 = new string[nodeList8.Count];
            int d = 0;
            foreach (XmlNode node8 in nodeList8)
            {
                num1[d] = node8.Attributes["name"]?.InnerText;
                d++;
            }
            for (d = 0; d < num1.Length; d++)
            {

                if (words[0] == num1[d].ToString())
                {

                    sim.Keyboard.TextEntry($"{FindInDictionary(words[0])}" + " ");
                    break;
                }
            }




            switch (words[0])
            {
                //INCLUDE "file_name"  2
                case "include":
                   /* if (words[1] == "iostream")
                    {
                        sim.Keyboard.TextEntry(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"#include < {words[1]} >\r")));
                        sim.Keyboard.TextEntry($"using namespace std;\r");
                        level++;
                        sim.Keyboard.TextEntry("int" + " " + "main" + "()\r{\r" + Tab(level) + "\r}\r");
                        sim.Keyboard.KeyPress(
                            new VirtualKeyCode[] {
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP
                            });
                    }*/
                    //else
                    //{
                        sim.Keyboard.TextEntry(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"#include < {words[1]} >\r")));                        
                   // }
                    break;
                    
                    
                
                //for datatype
               // case "add":
                 //   sim.Keyboard.TextEntry(rslt + " " + words[2] \r");
                 //   break;


                //USING_NAMESPACE "name_of_namespace"  2
                case "using_namespace":
                    sim.Keyboard.TextEntry($"using namespace {rslt};\r");
                    break;

                //FUNCTION "data_type" "Function_name" 3
                case "function":
                    level++;
                    sim.Keyboard.TextEntry(rslt + " " + words[2] + "()\r{\r" + Tab(level) + "\r}\r");
                    sim.Keyboard.KeyPress(
                        new VirtualKeyCode[] {
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP
                        });
                    break;

                case "add":
                    if(words[1] == "for")
                    {
                        sim.Keyboard.TextEntry($"for" + " " + "("  + ";" +  ";" +" "+")" + "\r{\r\r}\r");
                        sim.Keyboard.KeyPress(
                                    new VirtualKeyCode[] {
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP,
                            VirtualKeyCode.RIGHT,
                            VirtualKeyCode.RIGHT,
                            VirtualKeyCode.RIGHT,
                            VirtualKeyCode.RIGHT,
                            VirtualKeyCode.RIGHT,
                          }) ;
                    }
                    else if (words[1] == "if")
                    {
                        sim.Keyboard.TextEntry("if" + " " + "(" + " " + ")" + "{\r\r}\r");
                        sim.Keyboard.TextEntry("else" + " " + "{\r\r}\r");
                        sim.Keyboard.KeyPress(
                                    new VirtualKeyCode[] {
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP,
                            VirtualKeyCode.RIGHT,
                            VirtualKeyCode.RIGHT,
                            VirtualKeyCode.RIGHT,
                            VirtualKeyCode.RIGHT
                          }); ;
                    }
                    
                    else{
                        XmlNodeList list =  doc.GetElementsByTagName("Datatype");
                        foreach(XmlNode node in list)
                        {
                            if(node.Attributes["name"].Value == words[1])
                            {
                                sim.Keyboard.TextEntry($"{FindInDictionary(words[1])}" + " " + words[2]+"");
                                break;
                            }
                        }
                    }
                    break;

                //PRINT_LINE STRING/VAR "data_to_be_printed"  3
                case "print_line":
                    if (words[1] == "string")
                        sim.Keyboard.TextEntry($"cout<<\"{data}\"<<endl;\r");
                    else
                        sim.Keyboard.TextEntry($"cout<<{data}<<endl;\r");
                    break;

                //PRINT STRING/VAR "data_to_be_printed" 3
                case "print":
                    if (words[1] == "string")
                        sim.Keyboard.TextEntry($"cout<<\"{data}\";\r");
                    else
                        sim.Keyboard.TextEntry($"cout<<{data};\r");
                    break;

                case "sorry":
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_Z);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                   // sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    break;

                case "back":
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    break;

               

                case "erase":
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    break;

                case "clear":
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    break;

                case "left":
                    sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                    break;

                case "right":
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    break;

                case "up":
                    sim.Keyboard.KeyPress(VirtualKeyCode.UP);
                    break;

                case "down":
                    sim.Keyboard.KeyPress(VirtualKeyCode.DOWN);
                    break;

                case "newline":
                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    break;

                case "over":
                    sim.Keyboard.TextEntry(";");
                    break;

                case "lessthan":
                    sim.Keyboard.TextEntry("<");
                    break;

                case "lessthanorequalto":
                    sim.Keyboard.TextEntry("<=");
                    break;


                case "tab":
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    break;

                case "space":
                    sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                    break;

                case "backspace":
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    break;

                case "stop":
                    rec.RecognizeAsyncCancel();
                    recognizing = false;
                    statusBar.ToggleColor(recognizing);
                    break;

                case "exit":
                    OnExitEvent(this, new RoutedEventArgs());
                    break;
            }
        }

        protected virtual void OnExitEvent(object sender, RoutedEventArgs e)
        {
            ExitEvent?.Invoke(sender, e);
        }

        #region Private function

        private void Rec_Detected(object sender, SpeechDetectedEventArgs e)
        {
            Console.WriteLine(".....");
        }

        private void Rec_Completed(object sender, RecognizeCompletedEventArgs e)
        {
            //In multiple mode, raised when async recognition is canceled 
            Console.WriteLine("----------------------------");
        }

        private void Rec_AudioUpdate(object obj, AudioLevelUpdatedEventArgs e)
        {
            int maxBordersize = 7;
            statusBar.toggleBtn.BorderThickness = new Thickness(e.AudioLevel*maxBordersize/100 + 3);
        }

        private string FindInDictionary(string value)
        {
            if (CPPGrammar.dictionary.TryGetValue(value, out string str))
                return str;
            else
                return "<NotFound>";
        }

        private void OnToggle(object src, RoutedEventArgs e)
        {
            if (recognizing) { 
                rec.RecognizeAsyncCancel();
                recognizing = false;
                statusBar.status.Content = "Recognition Stop";
            }
            else {
                rec.RecognizeAsync(RecognizeMode.Multiple);
                recognizing = true;
                statusBar.status.Content = "I'm listening...";
            }
            statusBar.ToggleColor(recognizing);
        }

        private string Tab(int level)
        {
            string str = null;
            for (int i = 0; i < level; i++)
                str += '\t';
            Console.WriteLine(str.Replace("\t","TAB"));
            return str;
        }

        #endregion

        #region Public function

        public void StartRecognition(bool emulate)
        {
            if (emulate)
            {
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("include io_stream");
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("using_namespace standard");
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("function void main");
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("print_line string This string is going to be printed");
                Thread.Sleep(800);
                rec.EmulateRecognize("add ");
            }
            else
            {
                //Start recognizer
                rec.RecognizeAsync(RecognizeMode.Multiple);
                recognizing = true;
            }
        }

        public void StopRecognition()
        {
            //Start recognizer
            if (recognizing)
            {
                rec.RecognizeAsyncCancel();
                recognizing = false;
            }
        }

        public void Close()
        {
            StopRecognition();
            statusBar.Close();
        }

        public void ReloadGrammar()
        {
            if (recognizing)
                rec.RecognizeAsyncCancel();

            rec.UnloadAllGrammars();

            rec.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(cmd))));
            rec.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(aph))));
            rec.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(num))));
            rec.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(opr))));
            rec.LoadGrammar(new CPPGrammar().GetGrammar);

            if (recognizing)
                rec.RecognizeAsync(RecognizeMode.Multiple);

            Console.WriteLine("Reload Complete");
        }

        #endregion
    }
}