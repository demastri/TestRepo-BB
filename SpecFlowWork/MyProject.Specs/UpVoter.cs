using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Specs
{
    class UpVoter
    {
        private Dictionary<string, int> options;
        private List<string> sortedOptions;

        public UpVoter()
        {
            options = new Dictionary<string, int>();
            sortedOptions = new List<string>();
        }

        public void Add(string name)
        {
            Add(name, 0);
        }
        public void Add(string name, int curVotes)
        {
            // name should be unique...   ###

            options.Add(name, curVotes);
            Sort();
        }

        public void UpVote(string opt)
        {
            options[opt]++;
            Sort();
        }

        public void DownVote(string opt)
        {
            options[opt]--;
            Sort();
        }

        public void Sort()
        {
            sortedOptions.Clear();
            foreach (string o in options.Keys)
            {
                if (sortedOptions.Count == 0)
                    sortedOptions.Add(o);
                else
                    for( int i=0, thisCount=options[o]; i<sortedOptions.Count; i++ )
                        if (thisCount > options[sortedOptions[i]]) // ok, it belongs here
                        {
                            sortedOptions.Insert(i, o);
                            break;
                        }
            }
        }
        public string GetOption(int loc)
        {
            return sortedOptions[loc];
        }
    }
}
