using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UxlabFTPServer;

public class UxlabFTPTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //连接测试
        UxlabFTP.Instance.ConnectToFTPServer("nature", "Ttrby666");
        Debug.Log("<Test Case>Connect To FTP Server Test Success!");

        //相对路径下载test case
        StartCoroutine(UxlabFTP.Instance.DownloadFile("TestRelative.txt"));

        //绝对路径下载test case
        StartCoroutine(UxlabFTP.Instance.DownloadFile("ftp://127.0.0.1:14149/TestAbsolute.txt", Application.dataPath + "/SaveFiles/TestAbsolute.txt"));

        //遍历当前路径所有文件
        foreach(string tmp in UxlabFTP.Instance.GetFiles())
            Debug.Log(tmp);

        //遍历当前路径所有文件夹
        foreach (string tmp in UxlabFTP.Instance.GetDirectories())
            Debug.Log(tmp);

        //进入下一层级目录并下载文件
        UxlabFTP.Instance.GotoDirectory("Dir1");
        StartCoroutine(UxlabFTP.Instance.DownloadFile("file1-1.txt"));

        //返回上级目录
        UxlabFTP.Instance.FTPReturnToLastLevel();

        //相对路径上传文件
        StartCoroutine(UxlabFTP.Instance.UploadFile("UploadRelative.txt"));

        //绝对路径上传文件
        StartCoroutine(UxlabFTP.Instance.UploadFile(Application.dataPath + "/StreamingAssets/UploadFiles/UploadAbsolute.txt", "ftp://127.0.0.1:14149/UploadAbsolute.txt"));
    }
}
