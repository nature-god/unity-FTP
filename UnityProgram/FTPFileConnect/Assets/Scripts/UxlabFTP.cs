using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine.UI;

namespace UxlabFTPServer
{
    public class UxlabFTP : MonoBehaviour
    {
        private static UxlabFTP instance;
        public static UxlabFTP Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = GameObject.FindObjectOfType<UxlabFTP>();
                    if(instance == null)
                    {
                        GameObject go = new GameObject();
                        instance = go.AddComponent<UxlabFTP>();
                    }            
                }
                return instance;
            }
        }
        
        private string ftpUrlstring = null;             //FTP文件路径
        private NetworkCredential networkCredential;    //网络证书
        private string ftppath = "127.0.0.1:14149";     //FTP服务器地址
        private string currentDir = "/";                //用于记录当前层级目录

        private List<string> DirsList = new List<string>();         //获取FTP服务器中当前目录下的所有文件夹名称
        private List<string> FilesList = new List<string>();        //获取FTP服务器中当前目录下所有的文件


        #region 可调用的方法

        /// <summary>
        /// 连接FTP服务器，并在console界面展示连接返回结果
        /// </summary>
        public void ConnectToFTPServer(string userName,string password)
        {
            ftpUrlstring = "ftp://" + ftppath;

            networkCredential = new NetworkCredential(userName, password);
            
            if (ShowFtpFileAndDirectory() == true)
            {
                Debug.Log("Connect Success!");
            }
            else
            {
                Debug.Log("Connect Failed!");
            }
        }

        /// <summary>
        /// FTP返回上层级别
        /// </summary>
        public void FTPReturnToLastLevel()
        {
            string t = "";
            if (currentDir.EndsWith("/"))
            {
                string[] tmp = currentDir.Split('/');

                for (int i = 0; i < tmp.Length - 2; i++)
                {
                    if (tmp[i] == " ")
                        continue;
                    t += tmp[i];
                    t += "/";
                }
            }
            else
            {
                string[] tmp = currentDir.Split('/');

                for (int i = 0; i < tmp.Length - 1; i++)
                {
                    if (tmp[i] == " ")
                        continue;
                    t += tmp[i];
                    t += "/";
                }
            }
            currentDir = t;
            if (ShowFtpFileAndDirectory() == true)
            {
                Debug.Log("Connect Success!");
            }
            else
            {
                Debug.Log("Connect Failed!");
            }
        }

        /// <summary>
        /// 下载指定文件到指定路径下
        /// </summary>
        /// <param name="filename">待下载的文件名，相对路径</param>
        /// <param name="saveFileName">文件要存储的位置，绝对路径</param>
        /// <returns></returns>
        public IEnumerator DownloadFile(string filename, string saveFileName)
        {
            string savedFilePath = saveFileName;
            string url = GetUrlString(filename);
            try
            {
                FtpWebRequest resFile = CreatFtpWebRequest(url, WebRequestMethods.Ftp.GetFileSize);
                //resFile.UseBinary = true;
                //resFile.UsePassive = true;
                //resFile.Credentials = new NetworkCredential("nature", "Ttrby666");
                using (FtpWebResponse res = (FtpWebResponse)resFile.GetResponse())
                {
                    Debug.Log("All Length: " + res.ContentLength);
                    if (res == null)
                    {
                        yield break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Download filed: " + e.Message);
            }

            FtpWebRequest request = CreatFtpWebRequest(url, WebRequestMethods.Ftp.DownloadFile);
            FtpWebResponse response = GetFtpResponse(request);
            Stream responseStream = response.GetResponseStream();
            FileStream fileStream = File.Create(savedFilePath);
            int buflength = 8196;
            byte[] buffer = new byte[buflength];
            int bytesRead = 1;

            while (bytesRead != 0)
            {
                bytesRead = responseStream.Read(buffer, 0, buflength);
                fileStream.Write(buffer, 0, bytesRead);
                //Debug.Log("BytesRead:" + bytesRead + "AlreadyRead:" + downloadFileAlreadyLength);
                yield return new WaitForEndOfFrame();
            }

            Debug.Log("Download success!");
            fileStream.Close();
        }

        /// <summary>
        /// 获取当前FTP文件目录下文件夹
        /// </summary>
        /// <returns>返回FTP当前文件目录下的文件夹名称</returns>
        public List<string> GetDirectories()
        {
            return DirsList;
        }

        /// <summary>
        /// 获取当前FTP文件目录下的所有文件名
        /// </summary>
        /// <returns>返回FTP当前文件下的文件名称</returns>
        public List<string> GetFiles()
        {
            return FilesList;
        }

        #endregion


        #region 私有函数
        /// <summary>
        /// 创建FTP服务器连接
        /// </summary>
        /// <param name="url">FTP服务器地址</param>
        /// <param name="requestMethod">FTP服务器请求方法</param>
        /// <returns></returns>
        private FtpWebRequest CreatFtpWebRequest(string url, string requestMethod)
        {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(url);
            request.Credentials = networkCredential;
            request.KeepAlive = true;
            request.UseBinary = true;
            request.Method = requestMethod;
            return request;
        }

        /// <summary>
        /// 获取Ftp服务器返回的响应体
        /// </summary>
        /// <param name="request">发出的对FTP服务器请求</param>
        /// <returns></returns>
        private FtpWebResponse GetFtpResponse(FtpWebRequest request)
        {
            FtpWebResponse response = null;
            try
            {
                response = (FtpWebResponse)request.GetResponse();
                return response;
            }
            catch (WebException ex)
            {
                Debug.LogError(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 展示FTP连接后的遍历结果
        /// </summary>
        /// <returns>是否连接上FTP服务器</returns>
        private bool ShowFtpFileAndDirectory()
        {
            try
            {
                DirsList.Clear();
                FilesList.Clear();
                string url = string.Empty;
                if (currentDir == "/")
                {
                    url = ftpUrlstring;
                }
                else
                {
                    url = ftpUrlstring + currentDir;
                }
                Debug.Log(url);
                FtpWebRequest request = CreatFtpWebRequest(url, WebRequestMethods.Ftp.ListDirectoryDetails);
                FtpWebResponse response = GetFtpResponse(request);
                if (response == null)
                {
                    Debug.Log("Response is null");
                    return false;
                }

                //读取网络流数据
                Stream stream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream, Encoding.Default);
                string s = streamReader.ReadToEnd();
                streamReader.Close();
                response.Close();

                //处理并显示文件目录列表
                string[] ftpDir = s.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                int length = 0;
                for (int i = 0; i < ftpDir.Length; i++)
                {
                    if (ftpDir[i].EndsWith("."))
                    {
                        length = ftpDir[i].Length - 2;
                        break;
                    }
                }
                for (int i = 0; i < ftpDir.Length; i++)
                {
                    s = ftpDir[i];
                    int index = s.LastIndexOf('\t');
                    if (index == -1)
                    {
                        if (length < s.Length)
                        {
                            index = length;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    string name = s.Substring(index + 1);
                    if (name == "." || name == "..")
                    {
                        continue;
                    }
                    //判断是否为目录，在名称前加“Dirctory"来表示
                    if (s[0] == 'd' || (s.ToLower()).Contains("<dir>"))
                    {
                        string[] namefiled = name.Split(' ');
                        int namefiledlength = namefiled.Length;
                        string dirname;
                        dirname = namefiled[namefiledlength - 1];
                        dirname = dirname.PadRight(34, ' ');
                        name = dirname;
                        Debug.Log("<Dir>:" + name);
                        DirsList.Add(name.Replace(" ", ""));
                    }

                }
                for (int i = 0; i < ftpDir.Length; i++)
                {
                    s = ftpDir[i];
                    int index = s.LastIndexOf("\t");
                    if (index == -1)
                    {
                        if (length < s.Length)
                        {
                            index = length;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    string name = s.Substring(index + 1);
                    if (name == "." || name == "..")
                    {
                        continue;
                    }
                    if (!((s[0] == 'd') || (s.ToLower().Contains("<dir>"))))
                    {
                        string[] namefiled = name.Split(' ');
                        int namefiledlength = namefiled.Length;
                        string filename;
                        filename = namefiled[namefiledlength - 1];
                        filename = filename.PadRight(34, ' ');
                        name = filename;
                        Debug.Log("<File>:" + name);
                        FilesList.Add(name.Replace(" ", ""));
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 得到目标文件路径
        /// </summary>
        /// <param name="filename">目标文件名</param>
        /// <returns>目标文件的绝对文件路径</returns>
        private string GetUrlString(string filename)
        {
            string url = string.Empty;
            if (currentDir.EndsWith("/"))
            {
                url = ftpUrlstring + currentDir + filename;
            }
            else
            {
                url = ftpUrlstring + currentDir + "/" + filename;
            }
            return url;
        }
        #endregion
    }
}

