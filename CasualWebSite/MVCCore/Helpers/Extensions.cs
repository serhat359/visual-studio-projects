using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;

namespace MVCCore.Helpers;

public static class Extensions
{
    public static async Task<IEnumerable<T>> AwaitAllAsync<T>(this IEnumerable<Task<T>> source)
    {
        var res = new List<T>();
        foreach (var item in source)
        {
            res.Add(await item);
        }
        return res;
    }

    public static IEnumerable<XmlNode> GetAllNodesRecursive(this XmlNode node)
    {
        yield return node;

        foreach (XmlNode item in node.ChildNodes)
        {
            var allNodes = item.GetAllNodesRecursive();

            foreach (var newNode in allNodes)
            {
                yield return newNode;
            }
        }
    }

    public static XmlNode? SearchByTag(this XmlNode node, string name, string? @class = null)
    {
        var allNodes = node.GetAllNodesRecursive();
        if (@class == null)
            return allNodes.FirstOrDefault(c => c.TagName == name);
        else
            return allNodes.FirstOrDefault(c => c.TagName == name && c.Attributes?["class"] == @class);
    }

    public static string BeforeFirst(this string s, string needle)
    {
        var index = s.IndexOf(needle);

        if (index >= 0)
            return s.Substring(0, index);
        else
            return s;
    }

    public static async Task<HttpResponseMessage> SendWithRetryAsync(this HttpClient client, Func<HttpRequestMessage> requestCreator)
    {
        int failedCount = 0;
        while (true)
        {
            using var requestMessage = requestCreator();
            var response = await client.SendAsync(requestMessage);
            if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
            {
                return response;
            }

            response?.Dispose();

            failedCount++;

            var millisToBeWaited = Math.Pow(2, failedCount - 1) * 100;
            await Task.Delay((int)millisToBeWaited);
        }
    }
}
