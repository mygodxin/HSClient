using System;
using UnityEngine;

namespace HS
{
    /// <summary>
    /// �����������
    /// </summary>
    public class RandomUtil
    {
        /// <summary>
        /// �����������е�ĳһ��
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T RandomArrayElement<T>(T[] array)
        {
            return array[UnityEngine.Random.Range(0, array.Length)];
        }
        /// <summary>
        /// ���������е���
        /// </summary>
        public static void Shuffle<T>(T[] deck)
        {
            for (int i = 0; i < deck.Length; i++)
            {
                T temp = deck[i];
                int randomIndex = UnityEngine.Random.Range(i, deck.Length);
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
        }
        /// <summary>
        /// �齱
        /// </summary>
        /// <param name="probs"></param>
        /// <returns></returns>
        public static float Choose(float[] probs)
        {
            float total = 0;

            foreach (float elem in probs)
            {
                total += elem;
            }

            float randomPoint = UnityEngine.Random.value * total;

            for (int i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i])
                {
                    return i;
                }
                else
                {
                    randomPoint -= probs[i];
                }
            }
            return probs.Length - 1;
        }
        /// <summary>
        /// �齱
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T[] ChooseSet<T>(T[] array, int numRequired)
        {
            T[] result = new T[numRequired];

            int numToChoose = numRequired;

            for (int numLeft = array.Length; numLeft > 0; numLeft--)
            {

                float prob = (float)numToChoose / (float)numLeft;

                if (UnityEngine.Random.value <= prob)
                {
                    numToChoose--;
                    result[numToChoose] = array[numLeft - 1];

                    if (numToChoose == 0)
                    {
                        break;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// ��Ȩ�������ֵ
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static float CurveWeightedRandom(AnimationCurve curve)
        {
            return curve.Evaluate(UnityEngine.Random.value);
        }
    }
}
