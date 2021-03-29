#define debug
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Xml;
using System.IO;
using System;

namespace Voice_Coding.src
{
    class CPPGrammar
    {
        public static IDictionary<string, string> dictionary;
        //Choices for differet grammar

        enum Attribute
        {
            HeaderFiles,
            Namespace,
            Datatype,
            Other,
            PrintStyle,
            PrintValue,
            Operator,
            Alphabet,
            Number

        }

        private readonly GrammarBuilder includeBuilder = new GrammarBuilder();
        private readonly GrammarBuilder namespaceBuilder = new GrammarBuilder();
        private readonly GrammarBuilder functionBuilder = new GrammarBuilder();
        private readonly GrammarBuilder functionBuilder1 = new GrammarBuilder();
        private readonly GrammarBuilder printBuilder = new GrammarBuilder();
        private readonly GrammarBuilder otherBuilder = new GrammarBuilder();
        private readonly GrammarBuilder operatorBuilder = new GrammarBuilder();
        private readonly GrammarBuilder datatypeBuilder = new GrammarBuilder();
        private readonly GrammarBuilder alphabetBuilder = new GrammarBuilder();
        private readonly GrammarBuilder numberBuilder = new GrammarBuilder();

        private readonly Choices defaultChoice = new Choices(new string[] { "<Default Choice>" });
        private readonly Choices AllRules;

        public CPPGrammar()
        {
            dictionary = new Dictionary<string, string>();

            includeBuilder = new GrammarBuilder();
            includeBuilder.Append("include");
            includeBuilder.Append(GetChoice(Attribute.HeaderFiles));

            namespaceBuilder = new GrammarBuilder();
            namespaceBuilder.Append("using_namespace");
            namespaceBuilder.Append(GetChoice(Attribute.Namespace));

            functionBuilder = new GrammarBuilder();
            functionBuilder.Append("function");
            functionBuilder.Append(GetChoice(Attribute.Datatype));
            functionBuilder.AppendDictation();

            functionBuilder1 = new GrammarBuilder();
            functionBuilder1.Append("function");
            functionBuilder1.Append(GetChoice(Attribute.Datatype));
            functionBuilder1.Append(new Choices("main"));

            printBuilder = new GrammarBuilder();
            printBuilder.Append(GetChoice(Attribute.PrintStyle));
            printBuilder.Append(GetChoice(Attribute.PrintValue));
            printBuilder.AppendDictation();

            otherBuilder = new GrammarBuilder();
            otherBuilder.Append("add");
            otherBuilder.Append(GetChoice(Attribute.Other));
            
            datatypeBuilder = new GrammarBuilder();
            datatypeBuilder.Append("add");
            datatypeBuilder.Append(GetChoice(Attribute.Datatype));
            datatypeBuilder.Append(GetChoice(Attribute.Alphabet));

            alphabetBuilder = new GrammarBuilder();
            alphabetBuilder.Append(GetChoice(Attribute.Alphabet));

            numberBuilder = new GrammarBuilder();
            numberBuilder.Append(GetChoice(Attribute.Number));

            operatorBuilder = new GrammarBuilder();
            operatorBuilder.Append(GetChoice(Attribute.Operator));


            AllRules = new Choices(
                new GrammarBuilder[] {
                    includeBuilder,
                    namespaceBuilder,
                    functionBuilder,
                    functionBuilder1,
                    printBuilder,
                    otherBuilder,
                    operatorBuilder,
                    datatypeBuilder,
                    alphabetBuilder,
                    numberBuilder
                });
        }

        private Choices GetChoice(Attribute attribute)
        {
            XmlDocument doc = new XmlDocument();
#if debug
            doc.Load(@"D:\MY PROJECTS\8th sem project\Voice-Coding-EditChanges\res\MainResource.xml");
#else
            doc.Load(@"Resources\MainResource.xml");
#endif

            XmlNodeList nodeList = doc.GetElementsByTagName(attribute.ToString());

            SortedSet<string> dataList = new SortedSet<string>();

            if (attribute == Attribute.HeaderFiles)
            {
                foreach (XmlNode node in nodeList)
                {
                    if (node.Attributes["name"].Value.Equals("Path") && node.Attributes["type"].Value.Equals("custom"))
                    {
                        Console.WriteLine("Reading:" + node.Attributes["value"].Value);
                        DirectoryInfo dir = new DirectoryInfo(node.Attributes["value"].Value);

                        if (!dir.Exists)
                        {
                            Console.WriteLine("Directory not found, please provide existing directory path");
                            return defaultChoice;
                        }

                        DirectoryInfo[] dirArray = new DirectoryInfo[] { dir };
                        AddToList(dirArray, dataList);
                    }
                }
            }
            else
            {
                foreach (XmlNode node in nodeList)
                {
                    if(!dictionary.ContainsKey(node.Attributes["name"]?.InnerText))
                    dictionary.Add(node.Attributes["name"]?.InnerText, node.Attributes["value"]?.InnerText);
                    dataList.Add(node.Attributes["name"]?.InnerText);
                }
            }

            string[] str = new string[dataList.Count];
            dataList.CopyTo(str);

            return new Choices(str);
        }

        public Grammar GetGrammar
        {
            get
            {
                Grammar grammar = new Grammar(AllRules)
                {
                    Name = "VoiceCoding_CPP",
                };
                return grammar;
            }
        }
        
        private void AddToList(DirectoryInfo[] directoryInfos, SortedSet<string> list)
        {
            if (directoryInfos.Length == 0)
            {
                return;
            }
            foreach (DirectoryInfo dir in directoryInfos)
            {
                FileInfo[] files = dir.GetFiles("*");
                foreach (FileInfo file in files)
                {
                    list.Add(file.Name);
                }
                AddToList(dir.GetDirectories(), list);
            }
        }
    }
}