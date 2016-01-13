using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureboxWPF
{
    public class SecureboxKey
    {
        private byte[] _IV;
        private byte[] _KEY;
        private string _GUID;

        public byte[] IV
        {
            //set the person name
            set { this._IV = value; }
            //get the person name 
            get { return this._IV; }
        }
        public byte[] KEY
        {
            //set the person name
            set { this._KEY = value; }
            //get the person name 
            get { return this._KEY; }
        }
        public string GUID
        {
            //set the person name
            set { this._GUID = value; }
            //get the person name 
            get { return this._GUID; }
        }


    }
}
