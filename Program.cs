using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

//設定ファイルなどからユーザーリストを取得する想定
var userString = """
    honda.kousuke
    """;
var users = userString.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n')
    .Select((u, n) => (System.Text.RegularExpressions.Regex.Replace(u, @"^(.*?)\.(.*)$", "$2.$1"), n));



//入力した情報からADに問い合わせ
var domainName = Domain.GetCurrentDomain().Name;
string ldapPath = $"LDAP://{domainName}";
using var entry = new DirectoryEntry(ldapPath);

var userDetail = new List<string>(new string[users.Count()]);
Parallel.ForEach(users, info =>
{
    using var searcher = new DirectorySearcher(entry);
    searcher.Filter = $"(|(sAMAccountName=*{info.Item1}*)(mail=*{info.Item1}*)(displayName=*{info.Item1}*))";
    var results = searcher.FindOne();

    if (results != null)
    {
        var account = results.Properties["sAMAccountName"][0].ToString();
        var name = results.Properties["displayName"][0].ToString();
        var mail = results.Properties["mail"][0].ToString();
        var department = results.Properties["department"][0].ToString();
        userDetail[info.Item2] = $"{account},{name},{mail},{department}";

        Debug.WriteLine(userDetail[info.Item2]);
    }
});

foreach (var detail in userDetail)
{
    Console.WriteLine(detail);
}
