
using cmdNet.Model;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmdNet.Services;

public class ActiveDirectoryService
{
    private readonly string _username;
    private readonly string _password;
   
    public ActiveDirectoryService(
        string username,
        string password)
    {
        _username = username;
        _password = password;
    }
    public void ResetPassword(string _directory,
    string userName,
    string newPassword)
    {
        using DirectoryEntry root =
            new DirectoryEntry(
                _directory,
                _username,
                _password);

        using DirectorySearcher searcher =
            new DirectorySearcher(root);

        searcher.Filter =
            $"(&(objectClass=user)(sAMAccountName={userName}))";

        SearchResult result =
            searcher.FindOne();

        if (result == null)
            throw new Exception(
                $"User '{userName}' not found.");

        DirectoryEntry user =
            result.GetDirectoryEntry();

        user.Invoke(
            "SetPassword",
            new object[] { newPassword });

        int flags =
            (int)user.Properties["userAccountControl"].Value;

        flags &= ~0x2;       // Enable Account
        flags |= 0x10000;    // Password Never Expires

        user.Properties["userAccountControl"].Value =
            flags;

        user.CommitChanges();
    }

    public bool UserExists(
        string domainPath,
        string userName)
    {
        using DirectoryEntry entry =
            new(domainPath, _username, _password);

        using DirectorySearcher searcher =
            new(entry);

        searcher.Filter =
            $"(&(objectClass=user)(sAMAccountName={userName}))";

        searcher.PropertiesToLoad.Add("sAMAccountName");

        return searcher.FindOne() != null;
    }

    public void CreateUser(
        string ouPath,
        UserExcelModel user,
        string userName,
        string password)
    {
        //using DirectoryEntry ou =
        //    new($"LDAP://{ouPath}",
        //        _username,
        //        _password);
        using DirectoryEntry ou =
            new(ouPath,
                _username,
                _password);
        if (userName.Length > 20)
        {
            userName = userName.Substring(0, 20);
        }

        DirectoryEntry newUser =
            ou.Children.Add(
                $"CN={user.FirstName} {user.LastName}",
                "user");

        newUser.Properties["sAMAccountName"].Value =
            userName;

        newUser.Properties["userPrincipalName"].Value =
            $"{userName}@mcinext.org";

        newUser.Properties["givenName"].Value =
            user.FirstName;

        newUser.Properties["sn"].Value =
            user.LastName;

        newUser.Properties["displayName"].Value =
            $"{user.FirstName} {user.LastName}";

        if (!string.IsNullOrWhiteSpace(user.NationalCode))
            newUser.Properties["description"].Value =
            user.NationalCode;
        if (!string.IsNullOrWhiteSpace(user.Mobile))
            newUser.Properties["telephoneNumber"].Value =
            user.Mobile;

        newUser.CommitChanges();

        newUser.Invoke(
            "SetPassword",
            new object[] { password });

        const int ADS_UF_NORMAL_ACCOUNT = 512;
        const int ADS_UF_DONT_EXPIRE_PASSWD = 65536;

        newUser.Properties["userAccountControl"].Value =
            ADS_UF_NORMAL_ACCOUNT |
            ADS_UF_DONT_EXPIRE_PASSWD;

        newUser.CommitChanges();
    }



    private static int LevenshteinDistance(string s, string t)
    {
        int[,] d = new int[s.Length + 1, t.Length + 1];

        for (int i = 0; i <= s.Length; i++)
            d[i, 0] = i;

        for (int j = 0; j <= t.Length; j++)
            d[0, j] = j;

        for (int i = 1; i <= s.Length; i++)
        {
            for (int j = 1; j <= t.Length; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(
                        d[i - 1, j] + 1,
                        d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[s.Length, t.Length];
    }
    private static double CalculateSimilarity(
    string text1,
    string text2)
    {
        text1 = text1?.Trim().ToLower() ?? "";
        text2 = text2?.Trim().ToLower() ?? "";

        int distance =
            LevenshteinDistance(text1, text2);

        int maxLength =
            Math.Max(text1.Length, text2.Length);

        if (maxLength == 0)
            return 1;

        return 1.0 -
               ((double)distance / maxLength);
    }



    public AdUserInfo FindSimilarUser(
    string firstName,
    string lastName)
    {
        string excelDisplay =
            $"{firstName} {lastName}"
            .Trim()
            .ToLower();

        using DirectoryEntry root =
            new DirectoryEntry(
                "LDAP://DC=mcinext,DC=org",
                _username,
                _password);

        using DirectorySearcher searcher =
            new DirectorySearcher(root);

        searcher.Filter =
            "(&(objectCategory=person)(objectClass=user))";

        searcher.PropertiesToLoad.Add("givenName");
        searcher.PropertiesToLoad.Add("sn");
        searcher.PropertiesToLoad.Add("displayName");
        searcher.PropertiesToLoad.Add("sAMAccountName");

        AdUserInfo bestMatch = null;
        double bestScore = 0;

        foreach (SearchResult result in searcher.FindAll())
        {
            string givenName =
                result.Properties["givenName"].Count > 0
                    ? result.Properties["givenName"][0].ToString()
                    : "";

            string sn =
                result.Properties["sn"].Count > 0
                    ? result.Properties["sn"][0].ToString()
                    : "";

            string displayName =
                result.Properties["displayName"].Count > 0
                    ? result.Properties["displayName"][0].ToString()
                    : "";

            string adFullName =
                $"{givenName} {sn}"
                .Trim();

            double score1 =
                CalculateSimilarity(
                    excelDisplay,
                    adFullName);

            double score2 =
                CalculateSimilarity(
                    excelDisplay,
                    displayName);

            double score =
                Math.Max(score1, score2);

            if (score > bestScore)
            {
                bestScore = score;

                bestMatch = new AdUserInfo
                {
                    UserName =
                        result.Properties["sAMAccountName"]
                            .Count > 0
                            ? result.Properties["sAMAccountName"][0].ToString()
                            : "",

                    DisplayName =
                        displayName
                };
            }
        }

        // حداقل 80 درصد شباهت
        return bestScore >= 0.80
            ? bestMatch
            : null;
    }
    //public AdUserInfo FindSimilarUser(
    //string firstName,
    //string lastName)
    //{
    //    using DirectoryEntry root =
    //        new(
    //            "LDAP://DC=mcinext,DC=org",
    //            _username,
    //            _password);

    //    using DirectorySearcher searcher =
    //        new(root);

    //    searcher.Filter =
    //        "(&(objectCategory=person)(objectClass=user))";

    //    searcher.PropertiesToLoad.Add("givenName");
    //    searcher.PropertiesToLoad.Add("sn");
    //    searcher.PropertiesToLoad.Add("sAMAccountName");
    //    searcher.PropertiesToLoad.Add("displayName");

    //    foreach (SearchResult result in searcher.FindAll())
    //    {
    //        string first =
    //            result.Properties["givenName"].Count > 0
    //                ? result.Properties["givenName"][0].ToString()
    //                : "";

    //        string last =
    //            result.Properties["sn"].Count > 0
    //                ? result.Properties["sn"][0].ToString()
    //                : "";

    //        if (IsSimilar(
    //            firstName,
    //            lastName,
    //            first,
    //            last))
    //        {
    //            return new AdUserInfo
    //            {
    //                UserName =
    //                    result.Properties["sAMAccountName"][0]
    //                        .ToString(),

    //                DisplayName =
    //                    result.Properties["displayName"][0]
    //                        .ToString()
    //            };
    //        }
    //    }

    //    return null;
    //}
    private bool IsSimilar(
    string first1,
    string last1,
    string first2,
    string last2)
    {
        return
            LevenshteinDistance(
                first1.ToLower(),
                first2.ToLower()) <= 2

            &&

            LevenshteinDistance(
                last1.ToLower(),
                last2.ToLower()) <= 2;
    }
}