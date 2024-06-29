using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            var octanion = new double[] { -1, -1, -1 };
            var quaternionsQ = new double[] { -1, -1 };
            var complex = new double[] { -1 };
            var real = new double[] { };
            var QuaternionForm = new QuadraticForm(quaternionsQ);
            var ComplexForm = new QuadraticForm(complex);
            var OctForm = new QuadraticForm(octanion);
            var realForm = new QuadraticForm(real);
            // Создаем два элемента
            CliffordVector a = new CliffordVector(QuaternionForm, 4, 3, 2, 1);
            CliffordVector c = new CliffordVector(ComplexForm, 1, Math.PI/2 );
            CliffordVector o = new CliffordVector(OctForm, 1, 2, 3, 4, 5, 6, 7, 8);
            CliffordVector r = new CliffordVector(realForm, Math.PI );
            var a_ = a.CliffordConjugate();
            var c1_ = a_ * a;
            var c1 = a * a_;
            var a2 = a * a;
            Console.WriteLine(a);
            Console.WriteLine(a_);
            Console.WriteLine((a - a_) / 2);
            Console.WriteLine(c1_);
            Console.WriteLine(c1);
            Console.WriteLine(a2);

            Console.WriteLine(c);
            Console.WriteLine((c + c.CliffordConjugate()) / 2);

            Console.WriteLine((a * a.CliffordConjugate() + a.CliffordConjugate() * a) / 2);

            var o_ = o.FullConjugate();
            Console.WriteLine("oct");
            Console.WriteLine(o);
            Console.WriteLine(o_);
            Console.WriteLine(o * o);
            Console.WriteLine((o + o_) / 2);
            Console.WriteLine((o * o_).Re());

            Console.WriteLine("exp");
            var e = CliffordVector.Exp(r);
            Console.WriteLine(e);
            Console.WriteLine("err= " + (Math.Exp(r.GetCoefficient(0)) - e.GetCoefficient(0)).ToString());
            Console.WriteLine(CliffordVector.Exp(c));


            Console.ReadKey();
        }
    }

}
