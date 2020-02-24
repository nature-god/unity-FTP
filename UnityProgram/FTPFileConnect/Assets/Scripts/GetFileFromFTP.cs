using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine.UI;

public class GetFileFromFTP : MonoBehaviour {

    private const int ftpport = 21;                 //端口号
    private string ftpUrlstring = null;             //FTP文件路径
    private NetworkCredential networkCredential;    //网络证书
    private string ftppath = "127.0.0.1:14149";     //FTP服务器地址
    private string currentDir = "/";                //用于记录当前层级目录

    public string downname;

    public Transform UIPanel;                       //FTP服务器展示页面
    public GameObject DirPrefab;                    //文件夹Prefab
    public GameObject FilePrefab;                   //文件Prefab
    public GameObject GetBackButton;                //返回上级按钮

    private int anchorIndex = 0;
    private Vector2 anchorPos = new Vector2(-160.0f,0.0f);
    private Vector2 AnchorPos
    {
        get
        {           
            anchorPos.x = -160.0f + (80.0f * (anchorIndex % 5));
            anchorPos.y = 0.0f - (60.0f * (anchorIndex / 5));
            anchorIndex++;
            return anchorPos;
        }
    }


    /// <summary>
    /// 创建FTP服务器连接
    /// </summary>
    /// <param name="url">FTP服务器地址</param>
    /// <param name="requestMethod">FTP服务器请求方法</param>
    /// <returns></returns>
    public FtpWebRequest CreatFtpWebRequest(string url,string requestMethod)
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
    public FtpWebResponse GetFtpResponse(FtpWebRequest request)
    {
        FtpWebResponse response = null;
        try
        {
            response = (FtpWebResponse)request.GetResponse();
            return response;
        }
        catch(WebException ex)
        {
            Debug.LogError(ex.Message);
            return null;
        }
    }

    public void OnClickLoginButton()
    {
        ftpUrlstring = "ftp://" + ftppath;
        networkCredential = new NetworkCredential("nature", "Ttrby666");
        if(ShowFtpFileAndDirectory() == true)
        {
            Debug.Log("Connect Success!");
        }
        else
        {
            Debug.Log("Connect Failed!"); 
        }
    }

    public bool ShowFtpFileAndDirectory()
    {
        if(currentDir == "/")
        {
            GetBackButton.SetActive(false);
        }
        else
        {
            GetBackButton.SetActive(true);
        }

        for(int i=0;i<UIPanel.childCount;i++)
        {
            Destroy(UIPanel.GetChild(i).gameObject);
        }
        anchorIndex = 0;

        try
        {
            string url = string.Empty;
            if(currentDir == "/")
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

            if(response == null)
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
            //Debug.Log(s);

            //处理并显示文件目录列表
            string[] ftpDir = s.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach(string tmp in ftpDir)
            {
                Debug.Log(tmp);
            }
            int length = 0;
            for(int i = 0;i<ftpDir.Length;i++)
            {
                if(ftpDir[i].EndsWith("."))
                {
                    length = ftpDir[i].Length - 2;
                    break;
                }
            }


            for(int i = 0;i<ftpDir.Length;i++)
            {
                s = ftpDir[i];
                int index = s.LastIndexOf('\t');
                if(index == -1)
                {
                    if(length < s.Length)
                    {
                        index = length;
                    }
                    else
                    {
                        continue;
                    }
                }

                string name = s.Substring(index + 1);
                if(name == "."||name == "..")
                {
                    continue;
                }
                //判断是否为目录，在名称前加“Dirctory"来表示
                if(s[0] == 'd' || (s.ToLower()).Contains("<dir>"))
                {
                    string[] namefiled = name.Split(' ');
                    int namefiledlength = namefiled.Length;
                    string dirname;
                    dirname = namefiled[namefiledlength - 1];
                    dirname = dirname.PadRight(34, ' ');
                    name = dirname;
                    Debug.Log("<Dir>:" + name);
                    GameObject tmp = GameObject.Instantiate(DirPrefab, UIPanel);
                    tmp.GetComponentInChildren<Text>().text = name.Replace(" ", ""); ;
                    tmp.GetComponent<Button>().onClick.AddListener(
                        () => {
                            currentDir += (name.Replace(" ", "") + "/");
                            if (ShowFtpFileAndDirectory() == true)
                            {
                                Debug.Log("Connect Success!");
                            }
                            else
                            {
                                Debug.Log("Connect Failed!");
                            }
                        });
                    tmp.GetComponent<RectTransform>().anchoredPosition = AnchorPos;
                }

            }

            for(int i = 0;i < ftpDir.Length;i++)
            {
                s = ftpDir[i];
                int index = s.LastIndexOf("\t");
                if(index == -1)
                {
                    if(length < s.Length)
                    {
                        index = length;
                    }
                    else
                    {
                        continue;
                    }
                }
                string name = s.Substring(index + 1);
                if(name == "."||name == "..")
                {
                    continue;
                }
                if(!((s[0] == 'd')||(s.ToLower().Contains("<dir>"))))
                {
                    string[] namefiled = name.Split(' ');
                    int namefiledlength = namefiled.Length;
                    string filename;
                    filename = namefiled[namefiledlength - 1];
                    filename = filename.PadRight(34, ' ');
                    name = filename;
                    Debug.Log("<File>:" + name);

                    GameObject tmp = GameObject.Instantiate(FilePrefab, UIPanel);
                    tmp.GetComponentInChildren<Text>().text = name.Replace(" ", "");
                    tmp.GetComponent<Button>().onClick.AddListener(
                        () => {
                            DownloadFile(name);
                        });
                    tmp.GetComponent<RectTransform>().anchoredPosition = AnchorPos;

                }
            }
            return true;
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }

    public void ClickGetBackButton()
    {
        string t = "";
        if(currentDir.EndsWith("/"))
        {
            string[] tmp = currentDir.Split('/');

            for(int i = 0;i<tmp.Length-2;i++)
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

    public string GetUrlString(string filename)
    {
        string url = string.Empty;
        if(currentDir.EndsWith("/"))
        {
            url = ftpUrlstring + currentDir + filename;
        }
        else
        {
            url = ftpUrlstring + currentDir + "/" + filename;
        }
        return url;
    }

    public void DownloadFile(string filename)
    {
        string savedFilePath = Application.dataPath + "/SaveFiles/" + filename;
        try
        {
            string url = GetUrlString(filename);
            FtpWebRequest request = CreatFtpWebRequest(url, WebRequestMethods.Ftp.DownloadFile);
            FtpWebResponse response = GetFtpResponse(request);
            if(response == null)
            {
                return;
            }
            Stream responseStream = response.GetResponseStream();
            FileStream fileStream = File.Create(savedFilePath);
            int buflength = 8196;
            byte[] buffer = new byte[buflength];
            int bytesRead = 1;
            while(bytesRead != 0)
            {
                bytesRead = responseStream.Read(buffer, 0, buflength);
                fileStream.Write(buffer, 0, bytesRead);
            }

            Debug.Log("Download success!");
            fileStream.Close();
        }
        catch(Exception e)
        {
            Debug.Log("Download filed");
        }
    }
}
