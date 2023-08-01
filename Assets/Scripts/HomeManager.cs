using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SocketIO;

public class HomeManager : MonoBehaviour
{
    //simple형
    public GameObject simple_preparing;
    public GameObject simple_complete;
    public GameObject simple_prepare_Parent;
    public GameObject simple_complete_Parent;

    //image형
    public GameObject image_preparing;
    public GameObject image_Parent;
    public GameObject slideshow_img;
    public GameObject slideshow_Parent;

    //직원호출형
    public GameObject slideshow_call;
    public GameObject slideshow_call_Parent;
    public GameObject callForm;

    public GameObject simpleForm;
    public GameObject imageForm;
    public GameObject callclientForm;

    public GameObject socketPrefab;
    GameObject socketObj;
    SocketIOComponent socket;
    List<GameObject> simple_Obj = new List<GameObject>();
    List<GameObject> image_Obj = new List<GameObject>();
    bool is_socket_open = false;

    // Start is called before the first frame update
    void Start()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        if (Global.server_address == "")
        {
            Debug.Log("no saved data");
            SceneManager.LoadScene("setting");
        }
        else
        {
            Debug.Log(Global.setInfo.type);
            if (Global.setInfo.type == 1)
            {
                //단순형
                simpleForm.SetActive(true);
                imageForm.SetActive(false);
                callclientForm.SetActive(false);
            }
            else if(Global.setInfo.type == 0)
            {
                //이미지형
                simpleForm.SetActive(false);
                callclientForm.SetActive(false);
                imageForm.SetActive(true);
            }
            else
            {
                //직원호출형
                simpleForm.SetActive(false);
                callclientForm.SetActive(true);
                imageForm.SetActive(false);
            }

            //if(Global.setInfo.type != 2)
            //{
                LoadOrderlist();
            //}
            socketObj = Instantiate(socketPrefab);
            socket = socketObj.GetComponent<SocketIOComponent>();
            socket.On("open", socketOpen);
            socket.On("reloadOrder", ReloadOrerEventProcess);
            socket.On("error", socketError);
            socket.On("close", socketClose);
        }
    }

    void LoadOrderlist()
    {
        WWWForm form = new WWWForm();
        WWW www = new WWW(Global.api_url + Global.get_orderlist_api, form);
        StartCoroutine(GetOrderlistFromApi(www));
    }

    IEnumerator GetOrderlistFromApi(WWW www)
    {
        yield return www;
        if (www.error == null)
        {
            JSONNode jsonNode = SimpleJSON.JSON.Parse(www.text);
            Debug.Log(jsonNode);
            JSONNode prepare_list = JSON.Parse(jsonNode["receptOrderList"].ToString());
            for (int i = 0; i < prepare_list.Count; i++)
            {
                if (Global.setInfo.type == 1)
                {
                    //단순형
                    GameObject obj = Instantiate(simple_preparing);
                    obj.transform.SetParent(simple_prepare_Parent.transform);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                    string orderNo = Global.GetONoFormat(prepare_list[i]["orderSeq"].AsInt);
                    if (Global.setInfo.call_type == 1)
                    {
                        if (prepare_list[i]["tag_name"] != null && prepare_list[i]["tag_name"] != "" 
                            && prepare_list[i]["tag_name"] != "undefined")
                        {
                            orderNo = prepare_list[i]["tag_name"];
                        }
                    }

                    obj.transform.GetComponent<Text>().text = orderNo;
                    simple_Obj.Add(obj);
                }
                else if(Global.setInfo.type == 0)
                {
                    //이미지형
                    GameObject obj = Instantiate(image_preparing);
                    obj.transform.SetParent(image_Parent.transform);
                    obj.transform.localPosition = Vector3.zero;

                    string orderNo = Global.GetONoFormat(prepare_list[i]["orderSeq"]);
                    if (Global.setInfo.call_type == 1)
                    {
                        if (prepare_list[i]["tag_name"] != null && prepare_list[i]["tag_name"] != ""
                            && prepare_list[i]["tag_name"] != "undefined")
                        {
                            orderNo = prepare_list[i]["tag_name"];
                        }
                    }

                    obj.transform.Find("Text").GetComponent<Text>().text = orderNo;
                    if (image_Parent.transform.childCount > 1)
                    {
                        obj.transform.GetComponent<RectTransform>().sizeDelta =
                            new Vector2(image_Parent.transform.GetChild(0).transform.GetComponent<RectTransform>().sizeDelta.x,
                            image_Parent.transform.GetChild(0).transform.GetComponent<RectTransform>().sizeDelta.y);
                    }
                    else
                    {
                        Debug.Log(image_Parent.transform.GetComponent<RectTransform>().sizeDelta.x);
                        obj.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-image_Parent.transform.GetComponent<RectTransform>().sizeDelta.x, -image_Parent.transform.GetComponent<RectTransform>().sizeDelta.y / 5);
                    }
                    obj.transform.localScale = Vector3.one;
                    image_Obj.Add(obj);
                }
            }
            JSONNode complete_list = JSON.Parse(jsonNode["completeOrderList"].ToString());
            for (int i = 0; i < complete_list.Count; i++)
            {
                if (Global.setInfo.type == 1)
                {
                    //단순형
                    GameObject obj = Instantiate(simple_complete);
                    obj.transform.SetParent(simple_complete_Parent.transform);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                    string orderNo = Global.GetONoFormat(complete_list[i]["orderSeq"].AsInt);
                    if (Global.setInfo.call_type == 1)
                    {
                        if (complete_list[i]["tag_name"] != null && complete_list[i]["tag_name"] != ""
                            && complete_list[i]["tag_name"] != "undefined")
                        {
                            orderNo = complete_list[i]["tag_name"];
                        }
                    }

                    obj.transform.GetComponent<Text>().text = orderNo;
                    simple_Obj.Add(obj);
                }
            }
            if (Global.setInfo.type == 0)
            {
                //이미지형
                //UI Slideshow
                JSONNode pimgs = JSON.Parse(jsonNode["pubImages"].ToString());
                StartCoroutine(LoadSlideShow(pimgs));
            }else if(Global.setInfo.type == 2)
            {
                //직원호출형
                //UI Slideshow
                JSONNode pimgs = JSON.Parse(jsonNode["pubImages"].ToString());
                StartCoroutine(LoadSlideShowForCall(pimgs));
            }
        }
    }

    IEnumerator LoadSlideShowForCall(JSONNode pimgs)
    {
        for (int i = 0; i < pimgs.Count; i++)
        {
            GameObject slideobj = Instantiate(slideshow_call);
            slideobj.transform.SetParent(slideshow_call_Parent.transform);

            slideobj.transform.GetComponent<RectTransform>().anchorMin = Vector3.zero;
            slideobj.transform.GetComponent<RectTransform>().anchorMax = Vector3.one;
            float left = 0;
            float right = 0;
            slideobj.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2((left - right) / 2, 0f);
            slideobj.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-(left + right), 0);
            slideobj.transform.localScale = Vector3.one;
            if (pimgs.Count == 1)
            {
                slideobj.GetComponent<Image>().type = Image.Type.Simple;
            }
            else
            {
                slideobj.GetComponent<Image>().type = Image.Type.Sliced;
            }
            StartCoroutine(LoadImg(pimgs[i], slideobj));
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator LoadSlideShow(JSONNode pimgs)
    {
        for (int i = 0; i < pimgs.Count; i++)
        {
            GameObject slideobj = Instantiate(slideshow_img);
            slideobj.transform.SetParent(slideshow_Parent.transform);

            slideobj.transform.GetComponent<RectTransform>().anchorMin = Vector3.zero;
            slideobj.transform.GetComponent<RectTransform>().anchorMax = Vector3.one;
            float left = 0;
            float right = 0;
            slideobj.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2((left - right) / 2, 0f);
            slideobj.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-(left + right), 0);
            slideobj.transform.localScale = Vector3.one;
            if(pimgs.Count == 1)
            {
                slideobj.GetComponent<Image>().type = Image.Type.Simple;
            }
            else
            {
                slideobj.GetComponent<Image>().type = Image.Type.Sliced;
            }
            StartCoroutine(LoadImg(pimgs[i], slideobj));
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator LoadImg(string img_path, GameObject imgObj)
    {
        Debug.Log(img_path);
        WWW www = new WWW(img_path);
        yield return www;
        try
        {
            imgObj.transform.GetComponent<Image>().sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
            imgObj.transform.GetComponent<RectTransform>().up = new Vector3(0, 0, 0);
            imgObj.transform.GetComponent<RectTransform>().right = new Vector3(0, 0, 0);
            Global.is_loading_slideshow = true;
        }
        catch (Exception ex)
        {
            imgObj.transform.GetComponent<Image>().sprite = null;
        }
    }

    public void AddNewOrder(Order_Item oitem, int is_call)
    {
        if(oitem.status == 3)
        {
            //완료인 경우
            StartCoroutine(AddNeworderProcess(oitem, is_call));
        }
        else if(oitem.status == 2)
        {
            Debug.Log("조리중상태로 변경된 경우 " + oitem.orderNo);
            //조리중인 경우
            if (Global.setInfo.type == 1)
            {
                //단순형
                bool is_finding = false;
                for (int i = 0; i < simple_Obj.Count; i++)
                {
                    try
                    {
                        if (simple_Obj[i].gameObject.transform.GetComponent<Text>().text == oitem.orderNo)
                        {
                            is_finding = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                }
                if (!is_finding)
                {
                    GameObject obj = Instantiate(simple_preparing);
                    obj.transform.SetParent(simple_prepare_Parent.transform);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                    obj.transform.GetComponent<Text>().text = oitem.orderNo;
                    simple_Obj.Add(obj);
                }
            }
            else if(Global.setInfo.type == 0)
            {
                //이미지형인 경우
                bool is_finding = false;
                for (int i = 0; i < image_Obj.Count; i++)
                {
                    try
                    {
                        if (image_Obj[i].gameObject.transform.Find("Text").GetComponent<Text>().text == oitem.orderNo)
                        {
                            image_Obj[i].gameObject.transform.Find("back").GetComponent<Image>().sprite = Resources.Load<Sprite>("rect_black");
                            image_Obj[i].gameObject.transform.Find("Text").GetComponent<Text>().color = Color.white;
                            is_finding = true;
                            Debug.Log(oitem.orderNo + " already exists.");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                }
                if (!is_finding)
                {
                    Debug.Log("new created " + oitem.orderNo);
                    GameObject obj = Instantiate(image_preparing);
                    obj.transform.SetParent(image_Parent.transform);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.Find("Text").GetComponent<Text>().text = oitem.orderNo;
                    if(image_Parent.transform.childCount > 1)
                    {
                        obj.transform.GetComponent<RectTransform>().sizeDelta = 
                            new Vector2(image_Parent.transform.GetChild(0).transform.GetComponent<RectTransform>().sizeDelta.x,
                            image_Parent.transform.GetChild(0).transform.GetComponent<RectTransform>().sizeDelta.y);
                    }
                    else
                    {
                        Debug.Log(image_Parent.transform.GetComponent<RectTransform>().sizeDelta.x);
                        obj.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-image_Parent.transform.GetComponent<RectTransform>().sizeDelta.x, -image_Parent.transform.GetComponent<RectTransform>().sizeDelta.y / 5);
                    }
                    obj.transform.localScale = Vector3.one; 
                    image_Obj.Add(obj);
                }
            }
        }
        else if(oitem.status == 1)
        {
            //신규로 되돌리기된 경우
            if (Global.setInfo.type == 1)
            {
                //단순형
                for (int i = 0; i < simple_Obj.Count; i++)
                {
                    try
                    {
                        if (simple_Obj[i].gameObject.transform.GetComponent<Text>().text == oitem.orderNo)
                        {
                            DestroyImmediate(simple_Obj[i].gameObject);
                            simple_Obj.Remove(simple_Obj[i]); break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                }
            }
            else if(Global.setInfo.type == 0)
            {
                //이미지형
                for (int i = 0; i < image_Obj.Count; i++)
                {
                    try
                    {
                        if (image_Obj[i].gameObject.transform.Find("Text").GetComponent<Text>().text == oitem.orderNo)
                        {
                            DestroyImmediate(image_Obj[i].gameObject);
                            image_Obj.Remove(image_Obj[i]); break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                }
            }
        }
    }

    IEnumerator AddNeworderProcess(Order_Item oitem, int is_call)
    {
        Debug.Log(is_call);
        if(is_call == 1)
        {
            GameObject.Find("Audio Source").GetComponent<AudioSource>().Play();
        }
        if (oitem.type == 1 && oitem.status == 3)
        {
            Debug.Log("개별메뉴완료인 경우 = " + oitem.orderNo);
            //개별메뉴완료인 경우
            if (Global.setInfo.type == 1)
            {
                //단순형
                for(int i = 0; i < simple_complete_Parent.transform.childCount; i++)
                {
                    if(simple_complete_Parent.transform.GetChild(i).GetComponent<Text>().text == oitem.orderNo)
                    {
                        DestroyImmediate(simple_complete_Parent.transform.GetChild(i).gameObject);break;
                    }
                }
                GameObject obj = Instantiate(simple_complete);
                obj.transform.SetParent(simple_complete_Parent.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                obj.transform.GetComponent<Text>().text = oitem.orderNo;
                yield return new WaitForSeconds(30f);
                DestroyImmediate(obj);
            }
            else if(Global.setInfo.type == 0)
            {
                //이미지형인 경우
                for (int i = 0; i < image_Obj.Count; i++)
                {
                    try
                    {
                        if (image_Obj[i].gameObject.transform.Find("Text").GetComponent<Text>().text == oitem.orderNo)
                        {
                            image_Obj[i].gameObject.transform.Find("back").GetComponent<Image>().sprite = Resources.Load<Sprite>("rect_yellow");
                            image_Obj[i].gameObject.transform.Find("Text").GetComponent<Text>().color = Color.black;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                }
                //ArrangeImageObj();
                yield return new WaitForSeconds(30f);
                for(int i = 0; i < image_Obj.Count; i ++)
                { 
                    try
                    {
                        if (image_Obj[i].gameObject.transform.Find("Text").GetComponent<Text>().text == oitem.orderNo)
                        {
                            image_Obj[i].gameObject.transform.Find("back").GetComponent<Image>().sprite = Resources.Load<Sprite>("rect_black");
                            image_Obj[i].gameObject.transform.Find("Text").GetComponent<Text>().color = Color.white;
                            break;
                        }
                    }catch(Exception ex)
                    {
                        Debug.Log(ex);
                    }
                }
            }
            else
            {
                StopCoroutine(wait30Thread(callForm, callForm.transform.Find("Text").GetComponent<Text>()));
                callForm.SetActive(true);
                callForm.GetComponent<Image>().color = Color.black;
                callForm.transform.Find("Title").GetComponent<Text>().text = "조리완료";
                callForm.transform.Find("Text").GetComponent<Text>().text = oitem.tableName;
                StartCoroutine(wait30Thread(callForm, callForm.transform.Find("Text").GetComponent<Text>()));
            }
        }
        else if(oitem.type == 0 && oitem.status == 3)
        {
            //전체주문완료인 경우
            Debug.Log("전체주문완료인 경우");
            if (Global.setInfo.type == 1)
            {
                //단순형
                for(int i = 0; i < simple_Obj.Count; i++)
                {
                    try
                    {
                        if (simple_Obj[i].gameObject.transform.GetComponent<Text>().text == oitem.orderNo)
                        {
                            DestroyImmediate(simple_Obj[i].gameObject);
                            simple_Obj.Remove(simple_Obj[i]);break;
                        }
                    }catch(Exception ex)
                    {
                        Debug.Log(ex);
                    }
                }
                for (int i = 0; i < simple_complete_Parent.transform.childCount; i++)
                {
                    if (simple_complete_Parent.transform.GetChild(i).GetComponent<Text>().text == oitem.orderNo)
                    {
                        DestroyImmediate(simple_complete_Parent.transform.GetChild(i).gameObject); break;
                    }
                }
                GameObject obj = Instantiate(simple_complete);
                obj.transform.SetParent(simple_complete_Parent.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                obj.transform.GetComponent<Text>().text = oitem.orderNo;
                yield return new WaitForSeconds(30f);
                DestroyImmediate(obj);
            }
            else if(Global.setInfo.type == 0)
            {
                //이미지형인 경우
                for (int i = 0; i < image_Obj.Count; i++)
                {
                    if (image_Obj[i].gameObject.transform.Find("Text").GetComponent<Text>().text == oitem.orderNo)
                    {
                        image_Obj[i].gameObject.transform.Find("back").GetComponent<Image>().sprite = Resources.Load<Sprite>("rect_yellow");
                        image_Obj[i].gameObject.transform.Find("Text").GetComponent<Text>().color = Color.black;
                        break;
                    }
                }
                yield return new WaitForSeconds(30f);
                for(int i = 0; i < image_Obj.Count; i ++)
                { 
                    if (image_Obj[i].gameObject.transform.Find("Text").GetComponent<Text>().text == oitem.orderNo)
                    { 
                        DestroyImmediate(image_Obj[i].gameObject);
                        image_Obj.Remove(image_Obj[i]); break;
                    }
                }
            }
            else
            {
                StopCoroutine(wait30Thread(callForm, callForm.transform.Find("Text").GetComponent<Text>()));
                callForm.SetActive(true);
                callForm.GetComponent<Image>().color = Color.black;
                callForm.transform.Find("Title").GetComponent<Text>().text = "조리완료";
                callForm.transform.Find("Text").GetComponent<Text>().text = oitem.tableName;
                StartCoroutine(wait30Thread(callForm, callForm.transform.Find("Text").GetComponent<Text>()));
            }
        }
    }

    IEnumerator wait30Thread(GameObject form, Text objTxt)
    {
        yield return new WaitForSeconds(30f);
        objTxt.text = "";
        form.SetActive(false);
    }

    IEnumerator AddNewClientCall(int type, string tablename = "")
    {
        Debug.Log("add client call event.");
        if(type == 2)
        {
            //주문앱에서 직원호출
            if (Global.setInfo.type == 2)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().Play();
                callForm.SetActive(true);
                callForm.GetComponent<Image>().color = new Color(253/255f, 210/255f, 11/255f, 1);
                callForm.transform.Find("Title").GetComponent<Text>().text = "테이블 호출";
                callForm.transform.Find("Text").GetComponent<Text>().text = tablename;
                yield return new WaitForSeconds(30f);
                callForm.transform.Find("Text").GetComponent<Text>().text = "";
                callForm.SetActive(false);
            }
        }
        else if(type == 1)
        {
            //주방앱에서 직원호출
            GameObject.Find("Audio Source").GetComponent<AudioSource>().Play();
            if (Global.setInfo.type == 1)
            {
                //단순형
                GameObject obj = Instantiate(simple_complete);
                obj.transform.SetParent(simple_complete_Parent.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                obj.transform.GetComponent<Text>().text = "000";
                yield return new WaitForSeconds(30f);
                DestroyImmediate(obj);
            }
            else if (Global.setInfo.type == 0)
            {
                //이미지형
                GameObject obj = Instantiate(image_preparing);
                obj.transform.SetParent(image_Parent.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.Find("Text").GetComponent<Text>().text = "000";
                obj.transform.transform.Find("back").GetComponent<Image>().sprite = Resources.Load<Sprite>("rect_yellow");
                obj.transform.transform.Find("Text").GetComponent<Text>().color = Color.black;
                if (image_Parent.transform.childCount > 1)
                {
                    obj.transform.GetComponent<RectTransform>().sizeDelta =
                        new Vector2(image_Parent.transform.GetChild(0).transform.GetComponent<RectTransform>().sizeDelta.x,
                        image_Parent.transform.GetChild(0).transform.GetComponent<RectTransform>().sizeDelta.y);
                }
                else
                {
                    Debug.Log(image_Parent.transform.GetComponent<RectTransform>().sizeDelta.x);
                    obj.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(-image_Parent.transform.GetComponent<RectTransform>().sizeDelta.x, -image_Parent.transform.GetComponent<RectTransform>().sizeDelta.y / 5);
                }
                obj.transform.localScale = Vector3.one;
                image_Obj.Add(obj);
                yield return new WaitForSeconds(30f);
                image_Obj.Remove(obj);
                DestroyImmediate(obj);
            }
            else
            {
                callForm.SetActive(true);
                callForm.GetComponent<Image>().color = Color.black;
                callForm.transform.Find("Title").GetComponent<Text>().text = "주방호출";
                callForm.transform.Find("Text").GetComponent<Text>().text = "000";
                yield return new WaitForSeconds(30f);
                callForm.transform.Find("Text").GetComponent<Text>().text = "";
                callForm.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void socketOpen(SocketIOEvent e)
    {
        if (is_socket_open)
            return;
        is_socket_open = true;
        socket.Emit("callSetInfo");
        Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
    }

    IEnumerator GotoScene(string sceneName)
    {
        Global.is_loading_slideshow = false;
        if (socket != null)
        {
            socket.Close();
            socket.OnDestroy();
            socket.OnApplicationQuit();
        }
        if(socketObj != null)
        {
            DestroyImmediate(socketObj);
        }
        yield return new WaitForFixedUpdate();
        SceneManager.LoadScene(sceneName);
    }

    public void GotoSetting()
    {
        StartCoroutine(GotoScene("setting"));
    }

    public void ReloadOrerEventProcess(SocketIOEvent e)
    {
        Debug.Log("new event..");
        JSONNode jsonNode = SimpleJSON.JSON.Parse(e.data.ToString());
        Order_Item oitem = new Order_Item();
        oitem.is_client_call_type = jsonNode["is_client_call_type"].AsInt;
        Debug.Log(oitem.is_client_call_type);
        if(oitem.is_client_call_type == 1)
        {
            //1-주방앱에서 직원호출형
            Debug.Log("주방앱에서 직원호출형");
            StartCoroutine(AddNewClientCall(1));
        }else if(oitem.is_client_call_type == 2)
        {
            //2-주문앱에서 직원호출형
            Debug.Log("주문앱에서 직원호출형");
            StartCoroutine(AddNewClientCall(2, jsonNode["tableNo"]));
        }
        else
        {
            Debug.Log("일반호출형");
            oitem.orderNo = Global.GetONoFormat(jsonNode["orderNo"].AsInt);
            if (Global.setInfo.call_type == 1)
            {
                Debug.Log("태그명 호출=" + jsonNode["tagName"]);
                if(jsonNode["tagName"] != null && jsonNode["tagName"] != "" && jsonNode["tagName"] != "undefined")
                {
                    oitem.orderNo = jsonNode["tagName"];
                }
            }
            oitem.status = jsonNode["status"];
            oitem.type = jsonNode["type"];
            int is_call = jsonNode["is_call"];
            oitem.tableName = jsonNode["tableName"];
            AddNewOrder(oitem, is_call);
        }
    }

    public void socketError(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
    }

    public void socketClose(SocketIOEvent e)
    {
        is_socket_open = false;
        Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
    }

    public void OnApplicationQuit()
    {
        socket.Close();
        socket.OnDestroy();
        socket.OnApplicationQuit();
    }
}
