using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        if (PlayerPrefs.GetString("ip") != "")
        {
            Global.setInfo.type = PlayerPrefs.GetInt("type");
            Global.setInfo.call_type = PlayerPrefs.GetInt("ctype");
            Global.setInfo.bus_id = PlayerPrefs.GetString("bus_id");
            Global.server_address = PlayerPrefs.GetString("ip");
            Global.api_url = "http://" + Global.server_address + ":" + Global.api_server_port + "/";
            Global.socket_server = "ws://" + Global.server_address + ":" + Global.api_server_port;
        }
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("call");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
