//using System.Text.RegularExpressions;
//using System;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.UIElements;

///// <summary>
///// ʱ����ع�����
///// </summary>
//public class DateUtil
//{

///**��ʽ������ xx��xxʱxx��xx�� */
//public static FormatDate1(DateTime timestamp)
//{
//        Date d = new Date();
//    var expireTime = timestamp;
//    var expireDay = 0;
//    var str = '';
//    if (expireTime <= 0)
//    {
//        // �Ѿ�����
//    }
//    else if (expireTime > 0 && expireTime <= 60)
//    {
//        // ��
//        str = Math.floor(expireTime / 60) + '��' + str;
//    }
//    else if (expireTime > 60 && expireTime <= 60 * 60)
//    {
//        // ��
//        str = Math.floor(expireTime / (60 * 60)) + '��' + str;
//    }
//    else if (expireTime > 60 * 60 && expireTime <= 60 * 60 * 24)
//    {
//        // ʱ 
//        str = Math.floor(expireTime / (60 * 60 * 24)) + 'ʱ' + str;
//    }
//    else if (expireTime > 60 * 60 * 24)
//    {
//        // ��   
//        str = Math.floor(expireTime / (60 * 60 * 24)) + '��' + str;
//    }
//    return str;
//}
///**
// * ��������ת��Ϊ00:00:00��ʽ�ĸ�ʽ�ַ���
// * @param sec �����������룩
// * @param showHour �Ƿ���ʾ��Сʱ��λ��x:x:x/x:x
// * @param pad С��10ʱ�Ƿ�ʹ������0��λ
// * @returns 
// */
//public static getTimeStr(sec: number, showHour: boolean, pad: boolean): string
//{
//    if (sec == null || isNaN(sec) || sec < 0)
//    {
//        sec = 0;
//    }
//    sec = Math.floor(sec);
//    const snds: number = sec % 60;
//    const tm: number = Math.floor(sec / 60);
//    const minutes = tm % 60;
//    const hour: number = Math.floor(tm / 60);
//    const arr = showHour?[hour, minutes, snds] : [minutes, snds] ;
//    if (pad)
//    {
//        arr.forEach((v, idx) =>
//        {
//            if (v < 10)
//            {
//                arr[idx] = Util.padLeft(v, 2, "0");
//            }
//        })

//        }
//    const str = arr.join(':');
//    return str;
//}
//}
