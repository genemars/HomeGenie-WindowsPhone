using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace HomeGenie
{
    public enum WebRequestStatus { Completed, WebException, BeginWebRequestGeneralError, FinishWebRequestGeneralError };

    public class WebRequestCompletedArgs
    {
        public string ResponseText;
        public WebRequestStatus RequestStatus;

        public WebRequestCompletedArgs(string response)
        {
            ResponseText = response;
        }

        public WebRequestCompletedArgs(WebRequestStatus error)
        {
            RequestStatus = error;
        }
    }

    public class HTTPRequestQueue
    {
        public class QueueRequest
        {
            public string Url;
            public Action<WebRequestCompletedArgs> Callback;
            public string Id;

            public QueueRequest(string url, Action<WebRequestCompletedArgs> callback)
            {
                Url = "http://" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerAddress"] + url;
                Callback = callback;
            }
            public QueueRequest(string id, string url, Action<WebRequestCompletedArgs> callback)
            {
                Id = id;
                Url = "http://" + (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerAddress"] + url;
                Callback = callback;
            }
        }

        enum QueueStatus { Stopped, OnGoing }; // Queue can be in one of these states

        public List<QueueRequest> requests = new List<QueueRequest>();


        QueueStatus currentStatus;
        QueueRequest currentRequest;
        HttpWebRequest WebRequest;
        private static String AppSettingsFileName = "httpreq_queue.xml";

        public HTTPRequestQueue()
        {
            currentStatus = QueueStatus.Stopped;
            WebRequest = null;
        }

        public void AddToQueue(string id, string url, Action<WebRequestCompletedArgs> callback)
        {
            QueueRequest qr = null;
            try 
	        {	        
                qr = requests.First(r => r.Id == id);
	        }
	        catch (Exception) { }
            if (qr == null) requests.Add(new QueueRequest(id, url, callback));
            MakeNextRequest();
        }

        public void AddToQueue(string url, Action<WebRequestCompletedArgs> callback)
        {

            requests.Add(new QueueRequest(url, callback));
            MakeNextRequest();
        }

        public void Start()
        {
            MakeNextRequest();
        }

        public void StopAndSaveQueue() //This will stop request queue and save pending requests to a file. 
        {
            if (currentStatus == QueueStatus.OnGoing)
            {
                currentStatus = QueueStatus.Stopped;
                if (WebRequest != null)
                    WebRequest.Abort();
                requests.Insert(0, currentRequest);

            }
            Save();
        }

        //Private functions 
        void MakeNextRequest()
        {

            if (currentStatus == QueueStatus.OnGoing)
            {

                if (requests.Count() == 0)
                {
                    currentStatus = QueueStatus.Stopped;
                }
                else if (requests.Count() > 0 && WebRequest == null)
                {
                    MakeRequest();
                }

            }
            else if (currentStatus == QueueStatus.Stopped)
            {
                if (requests.Count() > 0 && WebRequest == null)
                {
                    currentStatus = QueueStatus.OnGoing;
                    MakeRequest();
                }
            }
        }

        void MakeRequest()
        {
            if (currentStatus == QueueStatus.Stopped)
                return;

            currentRequest = requests.First();

            try
            {
                WebRequest = (HttpWebRequest)HttpWebRequest.Create(currentRequest.Url);
                if (IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerUsername") &&
                    (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"] != "" &&
                    IsolatedStorageSettings.ApplicationSettings.Contains("RemoteServerPassword") &&
                    (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"] != "")
                {
                    WebRequest.Credentials = new NetworkCredential((string)IsolatedStorageSettings.ApplicationSettings["RemoteServerUsername"], (string)IsolatedStorageSettings.ApplicationSettings["RemoteServerPassword"]);
                }
                WebRequest.BeginGetResponse(new AsyncCallback(FinishWebRequest), this);
            }
            catch (WebException e)
            {
                WebRequest = null;
                if (currentRequest.Callback != null)
                {
                    currentRequest.Callback(new WebRequestCompletedArgs(WebRequestStatus.WebException));
                }
            }
            catch
            {
                WebRequest = null;
                if (currentRequest.Callback != null)
                {
                    currentRequest.Callback(new WebRequestCompletedArgs(WebRequestStatus.BeginWebRequestGeneralError));
                }
            }
            //
            if (WebRequest != null)
            {
                requests.RemoveAt(0);
            }
        }

        void RequestCompleted(QueueRequest request, string responseString)
        {
            if (request.Callback != null)
            {
                request.Callback(new WebRequestCompletedArgs(responseString));
            }
        }

        void FinishWebRequest(IAsyncResult result)
        {
            HTTPRequestQueue ptr = (HTTPRequestQueue)result.AsyncState;

            if (currentStatus == QueueStatus.Stopped)
            {
                ptr.WebRequest = null; //Queue is been stopped 
                return;
            }

            HttpWebRequest request = (HttpWebRequest)ptr.WebRequest;
            HttpWebResponse response = null;
            Stream streamResponse = null;
            StreamReader streamRead = null;
            WebRequestStatus status = WebRequestStatus.Completed;

            bool requestSuccess = true;

            try
            {
                // End the operation
                response = (HttpWebResponse)request.EndGetResponse(result);
                streamResponse = response.GetResponseStream();
                streamRead = new StreamReader(streamResponse);
                string responseString = streamRead.ReadToEnd();
                RequestCompleted(currentRequest, responseString); //Request completed succesfully

            }
            catch (WebException e)
            {
                requestSuccess = false;
                status = WebRequestStatus.WebException;
            }
            catch (Exception ex)
            {
                requestSuccess = false;
                status = WebRequestStatus.FinishWebRequestGeneralError;
            }
            finally
            {
                // Close stream objects
                if (streamResponse != null)
                    streamResponse.Close();
                if (streamRead != null)
                    streamRead.Close();
                if (response != null)
                    response.Close();

                ptr.WebRequest = null;

                if (requestSuccess)
                    MakeNextRequest();
                else
                {
                    if (currentRequest.Callback != null)
                    {
                        currentRequest.Callback(new WebRequestCompletedArgs(status));
                    }
                }
            }
        }
        void Save()
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream stream = null;
            try
            {
                stream = storage.CreateFile(AppSettingsFileName);

                XmlSerializer xml = new XmlSerializer(GetType());
                xml.Serialize(stream, this);
            }
            catch (Exception ex)
            { }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }
        public static HTTPRequestQueue Load()
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            HTTPRequestQueue tmpQueue;

            if (storage.FileExists(AppSettingsFileName))
            {
                IsolatedStorageFileStream stream = null;
                try
                {
                    stream = storage.OpenFile(AppSettingsFileName, FileMode.Open);
                    XmlSerializer xml = new XmlSerializer(typeof(HTTPRequestQueue));

                    tmpQueue = xml.Deserialize(stream) as HTTPRequestQueue;
                }
                catch (Exception ex)
                {
                    tmpQueue = new HTTPRequestQueue();
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
            }
            else
            {
                tmpQueue = new HTTPRequestQueue();
            }

            return tmpQueue;
        }


    }
}
