﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTyping
{
    internal class Differ
    {
        public class DiffData
        {
            public enum DiffState
            {
                Equal,
                Intermediate,
                Unequal,
            }

            public string Text { get; }
            public DiffState State { get; set; }

            public DiffData(string text, DiffState state)
            {
                Text = text;
                State = state;
            }
        }

        private static IEnumerable<char> DecomposeHangul(char ch)
        {
            var choseongTable = new List<string>
            {
                "ㄱ",
                "ㄱㄱ",
                "ㄴ",
                "ㄷ",
                "ㄷㄷ",
                "ㄹ",
                "ㅁ",
                "ㅂ",
                "ㅂㅂ",
                "ㅅ",
                "ㅅㅅ",
                "ㅇ",
                "ㅈ",
                "ㅈㅈ",
                "ㅊ",
                "ㅋ",
                "ㅌ",
                "ㅍ",
                "ㅎ",
            };
            var jungseongTable = new List<string>
            {
                "ㅏ",
                "ㅐ",
                "ㅑ",
                "ㅒ",
                "ㅓ",
                "ㅔ",
                "ㅕ",
                "ㅖ",
                "ㅗ",
                "ㅗㅏ",
                "ㅗㅐ",
                "ㅗㅣ",
                "ㅛ",
                "ㅜ",
                "ㅜㅓ",
                "ㅜㅔ",
                "ㅜㅣ",
                "ㅠ",
                "ㅡ",
                "ㅡㅣ",
                "ㅣ"
            };
            var jongseongTable = new List<string>
            {
                " ",
                "ㄱ",
                "ㄱㄱ",
                "ㄱㅅ",
                "ㄴ",
                "ㄴㅈ",
                "ㄴㅎ",
                "ㄷ",
                "ㄹ",
                "ㄹㄱ",
                "ㄹㅁ",
                "ㄹㅂ",
                "ㄹㅅ",
                "ㄹㅌ",
                "ㄹㅍ",
                "ㄹㅎ",
                "ㅁ",
                "ㅂ",
                "ㅂㅅ",
                "ㅅ",
                "ㅅㅅ",
                "ㅇ",
                "ㅈ",
                "ㅊ",
                "ㅋ",
                "ㅌ",
                "ㅍ",
                "ㅎ"
            };

            if (ch >= (char)0x3131 && ch <= (char)0x3163) // ch가 Hangul Compatibility Jamo 유니코드 블럭에 있음
            {
                return new List<char> { ch };
            }
            if (ch < (char)0xAC00 || ch > (char)0xD79F) // ch가 Hangul Syllables 유니코드 블럭에 없음
            {
                return new List<char>();
            }

            int code = ch - (char)0xAC00;
            var result = new List<char>();

            int choseongIndex = code / (21 * 28);
            result.AddRange(choseongTable[choseongIndex]);
            code %= 21 * 28;

            int jungseongIndex = code / 28;
            result.AddRange(jungseongTable[jungseongIndex]);
            code %= 28;

            int jongseongIndex = code;
            if (jongseongIndex != 0) result.AddRange(jongseongTable[jongseongIndex]);

            return result;
        }

        public IEnumerable<DiffData> Diff(string text1, string text2, string originalText1, bool preserveText1 = true)
        {
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
            {
                return new List<DiffData>();
            }

            int length = Math.Min(text1.Length, text2.Length);
            var result = new List<DiffData>();
            int i = 0;
            DiffData.DiffState currentState = DiffData.DiffState.Unequal;
            string tempString = "";
            for (; i < length; i++)
            {
                char ch1 = text1[i], ch2 = text2[i];
                var decomposed1 = new List<char>(DecomposeHangul(ch1));
                var decomposed2 = new List<char>(DecomposeHangul(ch2));

                DiffData.DiffState state;

                if (decomposed1.Any() && decomposed2.Any()) // ch1, ch2 둘 다 한글
                {
                    if (decomposed1.SequenceEqual(decomposed2))
                    {
                        state = DiffData.DiffState.Equal;
                    }
                    else if (decomposed1.Count < decomposed2.Count) // 도깨비불 현상의 가능성
                    {
                        if (i < originalText1.Length - 1 &&
                            decomposed1.SequenceEqual(decomposed2.Take(decomposed1.Count)) &&
                            decomposed2.Count >= 3)
                        {
                            var nextDecomposed = new List<char>(DecomposeHangul(originalText1[i + 1]));
                            state = nextDecomposed.Any() && decomposed2.Last() == nextDecomposed[0]
                                ? DiffData.DiffState.Equal
                                : DiffData.DiffState.Unequal;
                        }
                        else
                        {
                            state = decomposed1.SequenceEqual(decomposed2.Take(decomposed1.Count))
                                ? DiffData.DiffState.Intermediate
                                : DiffData.DiffState.Unequal;
                        }
                    }
                    else
                    {
                        state = decomposed2.SequenceEqual(decomposed1.Take(decomposed2.Count))
                            ? DiffData.DiffState.Intermediate
                            : DiffData.DiffState.Unequal;
                    }
                }
                else
                {
                    state = ch1 == ch2 ? DiffData.DiffState.Equal : DiffData.DiffState.Unequal;
                }

                if (i == 0) currentState = state;

                if (state == currentState)
                {
                    tempString += ch1;
                }
                else
                {
                    result.Add(new DiffData(tempString, currentState));
                    currentState = state;
                    tempString = preserveText1 ? ch1.ToString() : ch2.ToString();
                }
            }

            if (tempString != "")
            {
                result.Add(new DiffData(tempString, currentState));
            }
            if (text1.Length == text2.Length) return result;

            result.Add(new DiffData((text1.Length < text2.Length ? text2 : text1).Substring(i),
                                    DiffData.DiffState.Unequal));
            return result;
        }
    }
}