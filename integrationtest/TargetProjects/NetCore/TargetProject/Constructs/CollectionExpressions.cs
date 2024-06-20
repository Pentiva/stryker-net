using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TargetProject.Constructs;

public class TestClass {
    static ReadOnlySpan<int> Value => [1, 2, 3];

    public int[] M1() {
        int[] abc = {5, 5};
        int[] bcd = [ 1, ..abc, 3 ];
        return bcd;
    }

    public int[] M2() {
        int[] abc = {5, 5};
        var bcd = (int[])[1, ..abc, 3];
        return bcd;
    }

    public void M3() {
        Span<string> weekDays = [ "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" ];
        foreach (var day in weekDays) {
            Console.WriteLine(day);
        }
    }

    // Initialize private field:
    private static readonly IReadOnlyList<string> _months = [
        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov",
        "Dec"
    ];

    // property with expression body:
    public IEnumerable<int> MaxDays => [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];

    public int Sum(IEnumerable<int> values) => values.Sum();

    public int Example() {
        // As a parameter:
        int sum = Sum([ 1, 2, 3, 4, 5 ]);
        return sum;
    }

    public void M4() {
        string hydrogen = "H";
        string helium = "He";
        string lithium = "Li";
        string beryllium = "Be";
        string boron = "B";
        string carbon = "C";
        string nitrogen = "N";
        string oxygen = "O";
        string fluorine = "F";
        string neon = "Ne";
        string[] elements = [
            hydrogen, helium, lithium, beryllium, boron, carbon, nitrogen, oxygen,
            fluorine, neon
        ];
        foreach (var element in elements) {
            Console.WriteLine(element);
        }
    }

    public string[] M5() {
        string[] vowels = [ "a", "e", "i", "o", "u" ];
        string[] consonants = [
            "b", "c", "d", "f", "g", "h", "j", "k", "l", "m",
            "n", "p", "q", "r", "s", "t", "v", "w", "x", "z"
        ];
        string[] alphabet = [..vowels, ..consonants, "y" ];
        return alphabet;
    }

    public IEnumerable<int> M6() => Iter([ 1 ]);

    public IEnumerable<T> Iter<T>(IList<T> list) {
        foreach (var l in list) {
            yield return l;
        }
    }
}
