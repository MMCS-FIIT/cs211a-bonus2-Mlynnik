using System.Text;

namespace BoolFuncTGBot
{
    class Math_funcs
    {
        /// <summary>
        /// является ли поданная функция, сохраняющей ноль
        /// </summary>
        public bool Is_Saving_0 { get; private set; }

        /// <summary>
        /// является ли поданная функция, сохраняющей единицу
        /// </summary>
        public bool Is_Saving_1 { get; private set; }

        /// <summary>
        /// является ли поданная функция монотонной
        /// </summary>
        public bool Is_Monotone { get; private set; }

        /// <summary>
        /// является ли поданная функция линейной
        /// </summary>
        public bool Is_Linear { get; private set; }

        /// <summary>
        /// является ли поданная функция самодвойственной
        /// </summary>
        public bool Is_Self_dual { get; private set; }

        /// <summary>
        /// массив значений функции
        /// </summary>
        int[] vals;

        /// <summary>
        /// класс, работающий с поданной функцей
        /// </summary>
        public Math_funcs(string values)
        {
            vals = new int[values.Length];
            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = values[i] == '0' ? 0 : 1;
            }
            Is_Saving_0 = Is_P0();
            Is_Saving_1 = Is_P1();
            Is_Self_dual = Is_S();
            (Is_Linear, Is_Monotone) = Is_L_and_M();

            basis = Adding_to_Base();
        }


        /// <summary>
        /// проверяет является ли поданная функция, сохраняющей ноль
        /// </summary>
        bool Is_P0() => vals[0] == 0;

        /// <summary>
        /// проверяет является ли поданная функция, сохраняющей единицу
        /// </summary>
        bool Is_P1() => vals.Last() == 1;

        /// <summary>
        /// проверяет является ли поданная функция самодвойственной
        /// </summary>
        bool Is_S()
        {
            for (int i = 0; i < (vals.Length / 2); i++)
            {
                if (vals[i] == vals[^(i + 1)])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// треугольник паскаля
        /// </summary>
        int[] triangle_pascal;

        public string polynomial_Zhegalkin { get; private set; }

        /// <summary>
        /// проверяет функцию на монотонность и линейность
        /// </summary>
        (bool, bool) Is_L_and_M()
        {
            int l = vals.Length;
            int count_vars = 0;
            while (l != 1)
            {
                count_vars++;
                l /= 2;
            }

            var lst_sets = new List<int[]>();

            for (int i = 0; i < vals.Length; i++)
            {
                lst_sets.Add(new int[count_vars]);
                int t = i;
                int ind = 1;
                while (t > 0)
                {
                    lst_sets[i][^ind] = t % 2; ind++; t /= 2;
                }
            }

            //проверка монотонности (сравнение наборов значений)
            bool flag = true;
            for (int i = 0; i < vals.Length; i++)
            {
                if (vals[i] == 1)
                {
                    for (int j = i + 1; j < vals.Length; j++)
                    {
                        if (vals[j] == 0)
                        {
                            short count_greater = 0;
                            for (int k = 0; k < count_vars; k++)
                            {
                                if (lst_sets[i][k] <= lst_sets[j][k])
                                {
                                    count_greater++;
                                }
                            }
                            if (count_greater == count_vars) { flag = false; break; }
                        }
                    }
                }
                if (!flag)
                    break;
            }

            //проверка линейности
            //построение полинома

            //треугольник паскаля
            triangle_pascal = new int[vals.Length];
            int[] copy_vals = (int[])vals.Clone();
            for (int i = 0; i < copy_vals.Length; i++)
            {
                triangle_pascal[i] = copy_vals[0];

                for (int j = 0; j < copy_vals.Length - i - 1; j++)
                    copy_vals[j] = (copy_vals[j] + copy_vals[j + 1]) % 2;
            }

            //разложение в полином
            StringBuilder polinom = new StringBuilder();
            if (vals[0] == 1) polinom.Append("1⊕");

            for (int i = 0; i < triangle_pascal.Length; i++)
            {
                if (triangle_pascal[i] == 1)
                {
                    for (int j = 0; j < count_vars; j++)
                    {
                        if (lst_sets[i][j] == 1)
                            polinom.Append($"x{j + 1}^");
                    }
                    polinom.Remove(polinom.Length - 1, 1);
                    polinom.Append('⊕');
                }
            }

            if (polinom.Length > 0)
                polinom.Remove(polinom.Length - 1, 1);
            polynomial_Zhegalkin = polinom.ToString();


            


            if (polynomial_Zhegalkin.Contains('^'))
                return (false, flag);
            return (true, flag);
        }


        /// <summary>
        /// базис
        /// </summary>
        public Dictionary<string, string[]> basis { get; private set; }

        /// <summary>
        /// дополняет множество функций до базиса
        /// </summary>
        private Dictionary<string, string[]> Adding_to_Base()
        {
            var arr_extra_funcs = new string[5] { "1", "0", "¬x", "x1⊕x2⊕x3", "x1^x2⊕x1^x3⊕x2^x3" };
            Dictionary<string, string[]> dict_extra_funcs = new Dictionary<string, string[]>();
            dict_extra_funcs["1"] = new string[] { "&mdash;", "+", "+", "&mdash;", "+" };
            dict_extra_funcs["0"] = new string[] { "+", "&mdash;", "+", "&mdash;", "+" };
            dict_extra_funcs["¬x"] = new string[] { "&mdash;", "&mdash;", "+", "+", "&mdash;" };
            dict_extra_funcs["x1⊕x2⊕x3"] = new string[] { "+", "+", "+", "+", "&mdash;" };
            dict_extra_funcs["x1^x2⊕x1^x3⊕x2^x3"] = new string[] { "+", "+", "&mdash;", "+", "+" };

            var dict_res = new Dictionary<string, string[]>
            {
                { $"исходная функция f1({string.Join("", vals)})\nв виде полинома Жегалкина:\n{polynomial_Zhegalkin}", new string[] { Is_Saving_0 ? "+" : "&mdash;", Is_Saving_1 ? "+" : "&mdash;", Is_Linear ? "+" : "&mdash;", Is_Self_dual ? "+" : "&mdash;", Is_Monotone ? "+" : "&mdash;", } }
            };

            if (!Is_Saving_0 && !Is_Saving_1 && !Is_Self_dual && !Is_Monotone && !Is_Linear)
            {
                return dict_res;
            }
            else if (!Is_Saving_0)
            {
                if (Is_Self_dual || Is_Saving_1)
                    dict_res[arr_extra_funcs[1]] = dict_extra_funcs[arr_extra_funcs[1]];
                if (Is_Linear)
                    dict_res[arr_extra_funcs[4]] = dict_extra_funcs[arr_extra_funcs[4]];
                if (Is_Monotone)
                    dict_res[arr_extra_funcs[3]] = dict_extra_funcs[arr_extra_funcs[3]];
            }
            else if (!Is_Saving_1)
            {
                dict_res[arr_extra_funcs[0]] = dict_extra_funcs[arr_extra_funcs[0]];
                if (Is_Linear)
                    dict_res[arr_extra_funcs[4]] = dict_extra_funcs[arr_extra_funcs[4]];
                if (Is_Monotone)
                    dict_res[arr_extra_funcs[3]] = dict_extra_funcs[arr_extra_funcs[3]];
            }
            else if (!Is_Linear)
            {
                dict_res[arr_extra_funcs[0]] = dict_extra_funcs[arr_extra_funcs[0]];
                dict_res[arr_extra_funcs[1]] = dict_extra_funcs[arr_extra_funcs[1]];
                if (Is_Monotone)
                    dict_res[arr_extra_funcs[3]] = dict_extra_funcs[arr_extra_funcs[3]];
            }
            else if (!Is_Self_dual)
            {
                dict_res[arr_extra_funcs[2]] = dict_extra_funcs[arr_extra_funcs[2]];
                dict_res[arr_extra_funcs[4]] = dict_extra_funcs[arr_extra_funcs[4]];
            }
            else if (!Is_Monotone)
            {
                dict_res[arr_extra_funcs[1]] = dict_extra_funcs[arr_extra_funcs[1]];
                dict_res[arr_extra_funcs[2]] = dict_extra_funcs[arr_extra_funcs[2]];
                dict_res[arr_extra_funcs[4]] = dict_extra_funcs[arr_extra_funcs[4]];
            }
            return dict_res;
        }

        /// <summary>
        /// выгружает данные в локальный html файл
        /// </summary>
        public void OutPut_to_File()
        {
            StringBuilder str_output = new StringBuilder();

            str_output.Append("<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <meta charset=\"utf-8\">\r\n    <title>Таблица принадлежности классам Поста</title>\r\n    <style>\r\n        td {\r\n            width: 60px;\r\n            height: 60px;\r\n            border: solid 1px silver;\r\n            text-align: center;\r\n            font-size: medium;\r\n            font-weight: 600;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    <p>Таблица принадлежности классам Поста</p>\r\n    <table border=\"1\">\r\n        <tr>\r\n            <td></td>\r\n            <td>P0</td>\r\n            <td>P1</td>\r\n            <td>L</td>\r\n            <td>S</td>\r\n            <td>M</td>\r\n            <td>Комментарий</td>\r\n        </tr>");

            if (basis.Count <= 1)
            {
                var basis_val = basis.Values.First();
                str_output.Append($"<tr>\r\n            <td>f1</td>\r\n            <td>{basis_val[0]}</td>\r\n" +
                        $"            <td>{basis_val[1]}</td>\r\n            <td>{basis_val[2]}</td>\r\n            <td>{basis_val[3]}</td>\r\n" +
                        $"            <td>{basis_val[4]}</td>\r\n");
                if (basis_val[0] != "+")
                    str_output.Append("            <td>Функция уже является базисом</td>\r\n        </tr>");
                else
                    str_output.Append("            <td>Функцию нельзя дополнить до базиса</td>\r\n        </tr>");
            }
            else
            {
                int i = 1;
                foreach (var x in basis)
                {
                    str_output.Append($"<tr>\r\n            <td>f{i}</td>\r\n            <td>{x.Value[0]}</td>\r\n" +
                        $"            <td>{x.Value[1]}</td>\r\n            <td>{x.Value[2]}</td>\r\n            <td>{x.Value[3]}</td>\r\n" +
                        $"            <td>{x.Value[4]}</td>\r\n            <td>{x.Key}</td>\r\n        </tr>");
                    i++;
                }
            }


            str_output.Append("\r\n    </table>\r\n</body>\r\n</html>");
            File.WriteAllText("table.html", str_output.ToString(), Encoding.UTF8);
        }
    }
}
