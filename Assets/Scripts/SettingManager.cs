using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleJSON;
using System;

public class SettingManager : MonoBehaviour
{
    public GameObject optType;
    public InputField bus_id;
    public InputField pos_ip;
    public GameObject error_popup;
    public Text error_msg;
    public GameObject callType;

    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        if (Global.server_address != "")
        {
            pos_ip.text = Global.server_address;
            bus_id.text = Global.setInfo.bus_id;
            if (Global.setInfo.type == 1)
            {
                optType.transform.Find("simple").GetComponent<Toggle>().isOn = true;
            }
            else if(Global.setInfo.type == 0)
            {
                optType.transform.Find("image").GetComponent<Toggle>().isOn = true;
            }
            else
            {
                optType.transform.Find("callclient").GetComponent<Toggle>().isOn = true;
            }

            if(Global.setInfo.call_type == 0)
            {
                callType.transform.Find("order").GetComponent<Toggle>().isOn = true;
            }
            else
            {
                callType.transform.Find("tag").GetComponent<Toggle>().isOn = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onExit()
    {
        Application.Quit();
    }

    public void onBackBtn()
    {
        if(bus_id.text == "")
        {
            error_popup.SetActive(true);
            error_msg.text = "사업자번호를 정확히 입력하세요.";
            return;
        }
        if(pos_ip.text == "")
        {
            error_msg.text = "ip를 정확히 입력하세요.";
            error_popup.SetActive(true);
            return;
        }
        //api 통해서 인증
        Global.server_address = pos_ip.text;
        Global.api_url = "http://" + Global.server_address + ":" + Global.api_server_port + "/";
        Global.socket_server = "ws://" + Global.server_address + ":" + Global.api_server_port;
        WWWForm form = new WWWForm();
        form.AddField("bus_id", bus_id.text);
        WWW www = new WWW(Global.api_url + Global.verify_api, form);
        StartCoroutine(ProcessVerify(www));
    }

    IEnumerator ProcessVerify(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            string result = jsonNode["suc"].ToString()/*.Replace("\"", "")*/;
            Debug.Log(result);
            if (result == "1")
            {
                int option_type = 0;
                if (optType.transform.Find("simple").GetComponent<Toggle>().isOn)
                {
                    option_type = 1;
                }else if (optType.transform.Find("callclient").GetComponent<Toggle>().isOn)
                {
                    option_type = 2;
                }
                int c_type = 0;
                if (callType.transform.Find("tag").GetComponent<Toggle>().isOn)
                {
                    c_type = 1;
                }
                Global.setInfo.bus_id = bus_id.text;
                Global.setInfo.type = option_type;
                Global.setInfo.call_type = c_type;
                PlayerPrefs.SetString("bus_id", bus_id.text);
                PlayerPrefs.SetString("ip", Global.server_address);
                Global.api_url = "http://" + Global.server_address + ":" + Global.api_server_port + "/";
                Global.socket_server = "ws://" + Global.server_address + ":" + Global.api_server_port;
                PlayerPrefs.SetInt("type", option_type);
                PlayerPrefs.SetInt("ctype", c_type);
                SceneManager.LoadScene("call");
            }
            else
            {
                error_msg.text = jsonNode["msg"];
                error_popup.SetActive(true);
            }
        }
        else
        {
            error_msg.text = "인증에 실패하였습니다.";
            error_popup.SetActive(true);
        }
    }

    public void onComfirmErrorpopup()
    {
        error_popup.SetActive(false);
    }
}
