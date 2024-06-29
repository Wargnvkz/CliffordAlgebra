using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class QuadraticForm
{
    // Билинейная форма
    public double[] Form { get; set; }
    public QuadraticForm(double[] form)
    {
        SetQuadraticForm(form);
    }
    public void SetQuadraticForm(double[] form)
    {
        Form = form;
    }
}
public class CliffordVector
{
    public QuadraticForm QuadraticForm { get; set; }
    public int Dimension { get; set; }
    public double[] Coefficients { get; set; }

    public CliffordVector(QuadraticForm quadraticForm)
    {
        Init(quadraticForm);
    }

    public CliffordVector(QuadraticForm quadraticForm, params double[] coefficients)
    {
        Init(quadraticForm);
        Array.Copy(coefficients, Coefficients, coefficients.Length);
    }

    private void Init(QuadraticForm quadraticForm)
    {
        QuadraticForm = quadraticForm;
        Dimension = quadraticForm.Form.Length;
        Coefficients = new double[1 << Dimension];
    }

    public double GetCoefficient(int index)
    {
        return Coefficients[index];
    }

    public void SetCoefficient(int index, double value)
    {
        Coefficients[index] = value;
    }


    // Геометрическое произведение
    public CliffordVector GeometricProduct(CliffordVector other)
    {
        if (other.QuadraticForm != QuadraticForm)
            throw new ArgumentException("Должна использоваться одна билинейная форма");

        int n = 1 << Dimension; // Количество коэффициентов (2^Dimension)
        CliffordVector result = new CliffordVector(QuadraticForm);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                int index = i ^ j; // XOR для комбинирования базисных векторов
                double sign = CalculateSign(i, j);

                // Учитываем квадраты совпадающих базисных векторов
                int combined = i & j;
                if (combined != 0)
                {
                    for (int k = 0; k < Dimension; k++)
                    {
                        if ((combined & (1 << k)) != 0)
                        {
                            sign *= QuadraticForm.Form[k];
                        }
                    }
                }

                result.Coefficients[index] += sign * Coefficients[i] * other.Coefficients[j];
            }
        }

        return result;
    }

    // Метод для подсчета знака на основе числа инверсий битов
    // a и b - номера векторов
    private double CalculateSign(int a, int b)
    {
        int inversions = 0;
        for (int i = 0; i < Dimension; i++)
        {
            if ((b & (1 << i)) != 0)
            {
                for (int j = Dimension - 1; j > i; j--)
                {
                    if ((a & (1 << j)) != 0)
                    {
                        inversions++;
                    }
                }
            }
        }
        return (inversions % 2 == 0) ? 1.0 : -1.0;
    }


    public CliffordVector Commutator(CliffordVector other)
    {
        return this * other - other * this;// GeometricProduct(other).Subtract(other.GeometricProduct(this));
    }

    public CliffordVector Anticommutator(CliffordVector other)
    {
        return this * other + other * this;// GeometricProduct(other).Add(other.GeometricProduct(this));
    }

    public CliffordVector Add(CliffordVector other)
    {
        if (other.Dimension != Dimension)
            throw new ArgumentException("Должны иметь одинаковую размерность");

        CliffordVector result = new CliffordVector(QuadraticForm);
        for (int i = 0; i < Coefficients.Length; i++)
        {
            result.Coefficients[i] = Coefficients[i] + other.Coefficients[i];
        }
        return result;
    }

    public CliffordVector Subtract(CliffordVector other)
    {
        if (other.Dimension != Dimension)
            throw new ArgumentException("Должны иметь одинаковую размерность");

        CliffordVector result = new CliffordVector(QuadraticForm);
        for (int i = 0; i < Coefficients.Length; i++)
        {
            result.Coefficients[i] = Coefficients[i] - other.Coefficients[i];
        }
        return result;
    }

    public CliffordVector Reverse()
    {
        CliffordVector result = new CliffordVector(QuadraticForm);
        for (int i = 0; i < Coefficients.Length; i++)
        {
            int grade = GetGrade(i);
            result.Coefficients[i] = ((grade * (grade - 1) / 2) % 2 == 0) ? Coefficients[i] : -Coefficients[i];
        }
        return result;
    }
    public CliffordVector EvenConjugate()
    {
        CliffordVector result = new CliffordVector(QuadraticForm);
        for (int i = 0; i < Coefficients.Length; i++)
        {
            int grade = GetGrade(i);
            result.Coefficients[i] = (grade % 2 == 0) ? Coefficients[i] : -Coefficients[i];
        }
        return result;
    }

    public CliffordVector CliffordConjugate()
    {
        CliffordVector result = new CliffordVector(QuadraticForm);
        for (int i = 0; i < Coefficients.Length; i++)
        {
            int grade = GetGrade(i);
            result.Coefficients[i] = ((grade * (grade + 1) / 2) % 2 == 0) ? Coefficients[i] : -Coefficients[i];
        }
        return result;
    }

    public CliffordVector Normalize()
    {
        CliffordVector result = new CliffordVector(QuadraticForm);
        var sum = 0d;
        for (int i = 0; i < Coefficients.Length; i++)
        {
            sum += Coefficients[i] * Coefficients[i];
        }
        var norm = Math.Sqrt(sum);
        for (int i = 0; i < Coefficients.Length; i++)
        {
            result.Coefficients[i] = Coefficients[i] / norm;
        }
        return result;
    }

    public CliffordVector HermitianConjugate()
    {
        return this.EvenConjugate().Reverse();
    }
    public CliffordVector HermitianMultiply(CliffordVector other)
    {
        return this * other.HermitianConjugate();
    }

    public CliffordVector InnerAutomorphism(CliffordVector T)
    {
        return T * this * T.Inverse();
    }
    public CliffordVector TwistedInnerAutomorphism(CliffordVector T)
    {
        return T.EvenConjugate() * this * T.Inverse();
    }


    private int GetGrade(int index)
    {
        int grade = 0;
        while (index != 0)
        {
            grade += index & 1;
            index >>= 1;
        }
        return grade;
    }

    // Метод проектирования по степени
    public CliffordVector Project(int grade)
    {
        CliffordVector result = new CliffordVector(QuadraticForm);
        for (int i = 0; i < Coefficients.Length; i++)
        {
            if (GetGrade(i) == grade)
            {
                result.Coefficients[i] = Coefficients[i];
            }
        }
        return result;
    }

    public double Determinant()
    {
        switch (QuadraticForm.Form.Length)
        {
            case 0: return this.GetCoefficient(0);
            case 1: return (this * this.EvenConjugate()).GetCoefficient(0);
            case 2: return (this * this.EvenConjugate().Reverse()).GetCoefficient(0);
            case 3: return (this * this.Reverse() * this.EvenConjugate() * this.EvenConjugate().Reverse()).GetCoefficient(0);
            default: return double.NaN;
        }
    }

    public CliffordVector Inverse()
    {
        return this.HermitianConjugate() * (1 / (this.HermitianConjugate() * this).GetCoefficient(0));
    }
    /* public CliffordElement Inverse()
     {
         return this.Conjugate() * (1 / (this.Conjugate() * this).GetCoefficient(0));
     }

     public CliffordElement Conjugate()
     {
         CliffordElement result = new CliffordElement(QuadraticForm);
         for (int i = 0; i < Coefficients.Length; i++)
         {
             var sign = CalculateSign(i, i);
             for (int k = 0; k < Dimension; k++)
             {
                 if ((i & 1 << k) != 0) sign *= QuadraticForm.Form[k];
             }
             result.Coefficients[i] = sign * Coefficients[i];
         }
         return result;
     }
    */

    public CliffordVector FullConjugate()
    {
        CliffordVector result = new CliffordVector(QuadraticForm);
        for (int i = 0; i < Coefficients.Length; i++)
        {
            int grade = GetGrade(i);
            result.Coefficients[i] = grade == 0 ? Coefficients[i] : -Coefficients[i];
        }
        return result;
    }
    public CliffordVector Re(CliffordVector other)
    {
        return (this * other.FullConjugate() + other * this.FullConjugate()) / 2;
    }
    public CliffordVector Re()
    {
        return (this + this.FullConjugate()) / 2;
    }

    public static CliffordVector Exp(CliffordVector v)
    {
        var N = 30;
        var nFact = 1d;
        CliffordVector res = new CliffordVector(v.QuadraticForm);
        CliffordVector power = new CliffordVector(v.QuadraticForm);
        power.SetCoefficient(0, 1);
        for (int n = 0; n < N; n++)
        {
            if (n > 0) nFact *= n;
            res += power / nFact;
            power *= v;
        }
        return res;
    }


    public static int BitReverse(int v)
    {
        long res = 0;
        long highestBit = (long)1 << 30;
        while (v > 0)
        {
            var bit = v & 1;
            if (bit != 0)
            {
                res |= highestBit;
            }
            v >>= 1;
            highestBit >>= 1;
        }
        return (int)res;

    }

    public static string BitToVectors(int v)
    {
        var sb = new StringBuilder();
        var N = 1;
        while (v > 0)
        {
            var bit = v & 1;
            if (bit != 0)
            {
                sb.Append($"e{N}");
            }
            v >>= 1;
            N++;
        }
        return sb.Length == 0 ? "e0" : sb.ToString();
    }

    // Вывод элемента
    public override string ToString()
    {
        var lst = new List<Tuple<int, int, int>>();
        for (int i = 0; i < Coefficients.Length; i++)
        {
            var grade = GetGrade(i);
            lst.Add(new Tuple<int, int, int>(i, grade, BitReverse(i)));
        }
        return string.Join(" + ", lst.OrderBy(e => e.Item2).ThenByDescending(e1 => e1.Item3).Select(ee => $"{Coefficients[ee.Item1]}*{BitToVectors(ee.Item1)}"));
        //return string.Join(" + ", Coefficients);
    }

    public static CliffordVector operator *(CliffordVector c1, CliffordVector c2)
    {
        return c1.GeometricProduct(c2);
    }
    public static CliffordVector operator +(CliffordVector c1, CliffordVector c2)
    {
        return c1.Add(c2);
    }
    public static CliffordVector operator -(CliffordVector c1, CliffordVector c2)
    {
        return c1.Subtract(c2);
    }
    public static CliffordVector operator *(double v, CliffordVector c)
    {
        var vc = new CliffordVector(c.QuadraticForm);
        vc.SetCoefficient(0, v);
        return vc * c;
    }
    public static CliffordVector operator *(CliffordVector c, double v)
    {
        return v * c;
    }
    public static CliffordVector operator /(CliffordVector c, double v)
    {
        return 1 / v * c;
    }
}
