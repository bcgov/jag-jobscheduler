using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JobScheduler.Host
{
    internal static class ReflectionExtensions
    {
        private static readonly string[] excludedAssemblyPrefixes = new[] { "System.", "Microsoft." };
        private static readonly string[] assemblyFileExtensions = new[] { "*.dll" };

        public static async Task<string> GetManifestResourceString(this Assembly assembly, string manifestName)
        {
            using var stream = assembly.GetManifestResourceStream(manifestName);

            using var reader = new StreamReader(stream!);

            return await reader.ReadToEndAsync();
        }

        public static Type[] GetTypesImplementing<I>(this Assembly assembly) => assembly.GetTypesImplementing(typeof(I));

        public static Type[] GetTypesImplementing(this Assembly assembly, Type type) => assembly.DefinedTypes.Where(t => t.IsClass && !t.IsAbstract && t.IsPublic && (type.IsAssignableFrom(t) || t.IsAssignableToGenericType(type))).ToArray();

        public static I[] CreateInstancesOf<I>(this Assembly assembly) => assembly.GetTypesImplementing<I>().Select(t => (I)Activator.CreateInstance(t)!).ToArray();

        public static bool IsAssignableTo(this Type sourceType, Type destinationType, bool checkPolymorphicAssignability = false) => sourceType != typeof(object) && (destinationType.IsAssignableFrom(sourceType) || (checkPolymorphicAssignability && sourceType.BaseType!.IsAssignableTo(destinationType)));

        public static bool IsAssignableToGenericType(this Type type, Type genericType)
        {
            var interfaceTypes = type.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            var baseType = type.BaseType;
            return baseType != null && baseType.IsAssignableToGenericType(genericType);
        }

        public static async Task<object?> InvokeAsync(this MethodInfo method, object obj, params object[] parameters)
        {
            var task = (Task)(method.Invoke(obj, parameters) ?? null!);
            await task.ConfigureAwait(false);
            return method.ReturnType.IsGenericType
                ? task.GetType().GetProperty("Result")?.GetValue(task)
                : null;
        }

        public static IEnumerable<Assembly> DiscoverLocalAessemblies(string? directory = null, string? prefix = null)
        {
            if (string.IsNullOrEmpty(directory))
            {
                directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            var exclusedPrefixes = prefix != null ? excludedAssemblyPrefixes.Append(prefix).ToArray() : excludedAssemblyPrefixes;

            return assemblyFileExtensions.SelectMany(ext => Directory.GetFiles(directory!, ext, SearchOption.TopDirectoryOnly))
                .Where(file => !Array.Exists(exclusedPrefixes, prefix => Path.GetFileName(file).StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                .Select(file => Assembly.Load(AssemblyName.GetAssemblyName(file)))
                .ToList();
        }
    }
}