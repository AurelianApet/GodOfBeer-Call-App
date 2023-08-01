using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Order_Item
{
    public string orderNo;
    public string tableName;
    public int status;
    public int type;//0-주문전체처리, 1-메뉴개별처리
    public int is_client_call_type;//1-주방앱에서 직원호출형, 0-일반, 2-주문앱에서 직원호출
}

public struct Set_Info
{
    public int type;//1-단순형, 0-이미지형, 2-직원호출형
    public int call_type;//0-주문번호 호출, 1-tag 이름 호출
    public string bus_id;//사업자번호
}

public class Global
{
    //setting information
    public static Set_Info setInfo = new Set_Info();

    //slideshow value
    public static bool is_loading_slideshow = false;

    //api

    public static string server_address = "";
    public static string api_server_port = "3006";
    public static string api_url = "";
    static string api_prefix = "m-api/call/";

    //socket server
    public static string socket_server = "";

    public static string verify_api = api_prefix + "verify-info";
    public static string get_orderlist_api = api_prefix + "get-orderlist";
    public static string GetONoFormat(int ono)
    {
        return string.Format("{0:D3}", ono);
    }
}


