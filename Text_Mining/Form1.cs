using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Linq;

namespace Text_Mining
{
    public partial class Form1 : Form
    {
        string filePath = "";
        public List<Data> _list = new List<Data> { };

        int no_doc = 0;

        public struct Data
        {
            public Data(int intValue, string strValue, double tfidf)
            {
                IntegerData = intValue;
                StringData = strValue;
                DoubleData = tfidf;
            }

            public int IntegerData { get; private set; }
            public string StringData { get; private set; }
            public Double DoubleData { get; private set; }
        }

        private int Compare1(Data x, Data y)
        {
            return y.IntegerData.CompareTo(x.IntegerData);
        }
        private int Compare2(Data x, Data y)
        {
            return y.DoubleData.CompareTo(x.DoubleData);
        }

        public Form1()
        {
            InitializeComponent();
        }

        private List<Data> TF(String text) {
            no_doc = Regex.Matches(textBox1.Text, "\n", RegexOptions.IgnoreCase).Count;

            string delim = "\\ |/,.-+:[]{}\t،)'\"(!?<>«»:؟؛\r\n;";
            string[] words = text.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var word_query = (from string word in words orderby word select word).Distinct();
            string[] result = word_query.ToArray();
            var list = new List<Data>();

            for (int i = 0; i < result.Length; i++)
            {
                String searchTerm = result[i];
                var matchQuery = from word in words
                                 where word == searchTerm
                                 select word;
                double d = Math.Log((no_doc + 0.00) / matchQuery.Count());
                list.Add(new Data(matchQuery.Count(), searchTerm, d));
            }
            list.Sort(comparison: Compare2);
            return list;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(_list.Count == 0)
                _list = TF(textBox1.Text);


            WordCloud.WordCloud wc = new WordCloud.WordCloud(1024, 768, true);


            List<String> _words = new List<string> { };
            List<int> _frequencies = new List<int> { };
            for (int i = 0; i < 20; i++)
            {
                _words.Add(_list[i].StringData);
                _frequencies.Add(_list[i].IntegerData);
            }

            wc.Draw(_words, _frequencies).Save(@".\WordCloud.JPG");

            Process.Start("WordCloud.jpg");

        }


        private void button2_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }
            textBox1.Text = fileContent.ToLower();
            _list.Clear();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (_list.Count == 0)
                _list = TF(textBox1.Text);
            TextWriter tw = new StreamWriter("TFIDF.txt", false, Encoding.Unicode);
            string s = Encoding.UTF8.GetString(new byte[] { 239, 187, 191 });
            tw.Write(s);
            foreach (Data d in _list)
                tw.WriteLine(d.DoubleData.ToString() + "    " + d.StringData);
            tw.Flush();
            tw.Close();

            Process.Start("TFIDF.txt");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (_list.Count == 0)
                _list = TF(textBox1.Text);

            _list.Sort(comparison: Compare1);

            WordCloud.WordCloud wc = new WordCloud.WordCloud(1024, 768, true);


            List<String> _words = new List<string> { };
            List<int> _frequencies = new List<int> { };
            for (int i = 0; i < 20; i++)
            {
                _words.Add(_list[i].StringData);
                _frequencies.Add(_list[i].IntegerData);
            }

            wc.Draw(_words, _frequencies).Save(@".\Word_Cloud.JPG");

            Process.Start("Word_Cloud.jpg");

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (_list.Count == 0)
                _list = TF(textBox1.Text);
            string word = textBox2.Text.Trim();
            int before = (int)numericUpDown1.Value;
            int after = (int)numericUpDown2.Value;
            string delim = "\\ |/,.-+:[]{}\t،)'\"(!?<>«»:؟؛\r\n;";
            string corpus = textBox1.Text;
            string[] lines = corpus.Split('\n');

            List<String> paradigms_b = new List<String>() { };
            List<String> paradigms_a = new List<String>() { };

            foreach (String line in lines)
            {
                string[] sentences = line.Split(';');
                foreach (string sentence in sentences)
                {
                    string[] words = sentence.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    int res = Array.IndexOf(words, word);
                    if (res > -1)
                    {
                        if (before == -1)
                        {
                            for (int i = 0; i < res; i++)
                                if (_list.Find(x => x.StringData == words[i]).DoubleData > 1.5)
                                    paradigms_b.Add(words[i]);
                        }
                        else
                            for (int i = 1; i <= before; i++)
                                if (res - i > 0 && _list.Find(x => x.StringData == words[res - i]).DoubleData > 1.5)
                                    paradigms_b.Add(words[res - i]);
                        if (after == -1)
                        {
                            for (int i = res + 1; i < words.Length; i++)
                                if (_list.Find(x => x.StringData == words[i]).DoubleData > 1.5)
                                    paradigms_a.Add(words[i]);
                        }
                        else
                            for (int i = 1; i <= after; i++)
                                if (res + i < words.Length && _list.Find(x => x.StringData == words[res + i]).DoubleData > 1.5)
                                    paradigms_a.Add(words[res + i]);

                    }
                }
            }
            var par_b = paradigms_b.Distinct().ToArray();
            var par_a = paradigms_a.Distinct().ToArray();

            List<String> similar_words = new List<string>() { };
            foreach (String line in lines)
            {
                foreach (string b in par_b)
                {
                    foreach (string a in par_a)
                    {
                        string sentence = Regex.Match(line, b + @" (\w.*?) " + a).Groups[1].Value;
                        if (sentence == "" || sentence.Contains(' ') || _list.Find(x => x.StringData == sentence).DoubleData < 1.5)
                            continue;
                        //foreach (string w in sentence.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                        //similar_words.Add(w);
                        similar_words.Add(sentence);
                    }
                }
            }
            listBox1.Items.Clear();
            var distinct_words = similar_words.Distinct().ToArray();
            foreach (string dis in distinct_words)
                listBox1.Items.Add(dis);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*
            using (StreamReader reader = new StreamReader("hamshahri.txt"))
            {
                var fileContent = reader.ReadToEnd();
                textBox1.Text = fileContent.ToLower();
            }
            _list = TF(textBox1.Text);
            */
        }
    }
}
