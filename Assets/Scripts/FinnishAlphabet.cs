using UnityEngine;
using System.Text;

/// <summary>
/// A class to handle all things related to the Finnish alphabet.
/// </summary>
public class FinnishAlphabet : MonoBehaviour {

    /// <summary>
    /// Returns a random uppercase letter from the Finnish alphabet.
    /// </summary>
    /// <returns>A random Finnish letter.</returns>
    public static char RandomCharacter() {
        int seed = Random.Range(0, 29);
        switch (seed) {
            case 26:
                return 'Å';
            case 27:
                return 'Ä';
            case 28:
                return 'Ö';
            default:
                return char.ToUpper((char)('a' + seed));
        }
    }

    /// <summary>
    /// Convert an integer value to the equivalent character in the finnish alphabet, where the integers value is the alphabet position.
    /// A = 1, B = 2, etc...
    /// </summary>
    /// <param name="letter"></param>
    /// <returns>The character at parameters index within the Finnish alphabet.</returns>
    public static char GetCharacter(int letter) {
        switch (letter) {
            case 26:
                return 'Å';
            case 27:
                return 'Ä';
            case 28:
                return 'Ö';
            default:
                return char.ToUpper((char)('a' + letter));
        }
    }

    /// <summary>
    /// Nicely formats a word into it's correct gramatical stance.
    /// </summary>
    /// <param name="word">NICEIFY.</param>
    /// <returns>Niceify.</returns>
    public static string Niceify(string word) {
        char[] s = word.ToLower().ToCharArray();
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < s.Length; i++) {
            sb.Append(i == 0 ? s[i].ToString().ToUpper() : s[i].ToString());
        }

        return sb.ToString();
    }

}
