using System;
using System.Collections;
using System.Text.Json;

namespace ClientLibrary.Helpers;

public static class Serializations
{
    public static string Serializeobj<T>(T modelObj) => JsonSerializer.Serialize(modelObj);
    public static T DeserializeJsonString<T>(string jsonSrting) => JsonSerializer.Deserialize<T>(jsonSrting);
    public static IList<T> DeserializeJsonStringList<T>(string jsonSrting) => JsonSerializer.Deserialize<IList<T>>(jsonSrting);

}
