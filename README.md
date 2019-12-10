# JavaPropertiesTokenizer
JavaPropertiesTokenizer is a .NET library for tokenizing Java properties files.

It lets you load a Java properties file, edit it and write back the changes. It preseves the whitespace and separator characters so that the file can be faithfully recrated.

## Usage
```C#
using JavaPropertiesTokenizer;

// Basic reading of properties file to get only key value pairs
Dictionary<string, string> props = JavaPropertiesParser.ParseFileToDictionary(@"C:\Path\File.properties");
// or
Dictionary<string, string> props = JavaPropertiesParser.ParseFileToDictionary(@"C:\Path\File.properties", JavaPropertiesParser.DefaultEncoding, true);

// Reading tokens of a properties file
using var fileStream = File.Open(@"C:\Path\File.properties", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
var fileTokens = JavaPropertiesParser.Parse(fileStream);

// Writing tokens to a properties file
using (var writer = new StreamWriter(@"C:\Path\File2.properties", false, JavaPropertiesParser.DefaultEncoding))
{
    JavaPropertiesWriter.Write(writer, fileTokens);
}

```

## License
This library is licensed under the [MIT](LICENSE.TXT) license.
