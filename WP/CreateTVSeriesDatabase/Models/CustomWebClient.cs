using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO;

namespace CreateTVSeriesDatabase.Models
{
    public class CustomWebClient : WebClient
    {
        private string mURI;
        private EventHandler mEventHandler;
        private object mData;

        public CustomWebClient(string URI, EventHandler eventHandler, object data)
        {
            mURI = URI;
            mEventHandler = eventHandler;
            mData = data;
        }

        public CustomWebClient(string URI)
        {
            mURI = URI;
            mEventHandler = null;
        }

        public CustomWebClient(string URI, object data)
        {
            mURI = URI;
            mEventHandler = null;
            mData = data;
        }

        public string GetURI
        {
            get
            {
                return mURI;
            }
        }

        public EventHandler GetEventHandler
        {
            get
            {
                return mEventHandler;
            }
        }

        public object GetData
        {
            get
            {
                return mData;
            }
        }

        public string DownloadString()
        {
            return this.DownloadString(new Uri(mURI));
        }

        public Stream DownloadStreamAsync()
        {
            return this.OpenRead(new Uri(mURI));
        }

    }
}
