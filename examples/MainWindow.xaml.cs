using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace cmdwtf.Smurves.Example;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}
}

public static class AppExtentions
{
	public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> receipent)
		=> receipent(input);

	public static IEnumerable<TResult> SelectWithPrev<TSource, TResult>
		(this IEnumerable<TSource> source,
		Func<TSource, TSource?, int, TResult> projection)
	{
		using IEnumerator<TSource> iterator = source.GetEnumerator();
		int index = 0;
		TSource? previous = default;

		while (iterator.MoveNext())
		{
			yield return projection(iterator.Current, previous, index++);
			previous = iterator.Current;
		}
	}

	public static TOut? Clone<TOut>(this TOut control, bool autoParent = true) where TOut : FrameworkElement
	{
		try
		{
			using MemoryStream stream = new();
			XamlWriter.Save(control, stream);
			stream.Seek(0, SeekOrigin.Begin);
			TOut clone = (TOut)XamlReader.Load(stream);

			if (clone != null && autoParent && control.Parent is Panel parentable)
			{
				parentable.Children.Add(clone);
			}

			return clone;
		}
		catch
		{
			return null;
		}
	}

	public static BindingBase CloneViaXamlSerialization(this BindingBase binding)
	{
		var sb = new StringBuilder();
		var writer = XmlWriter.Create(sb, new XmlWriterSettings
		{
			Indent = true,
			ConformanceLevel = ConformanceLevel.Fragment,
			OmitXmlDeclaration = true,
			NamespaceHandling = NamespaceHandling.OmitDuplicates,
		});
		var mgr = new XamlDesignerSerializationManager(writer);

		// HERE BE MAGIC!!!
		mgr.XamlWriterMode = XamlWriterMode.Expression;
		// THERE WERE MAGIC!!!

		System.Windows.Markup.XamlWriter.Save(binding, mgr);
		StringReader stringReader = new StringReader(sb.ToString());
		XmlReader xmlReader = XmlReader.Create(stringReader);
		object newBinding = XamlReader.Load(xmlReader);

		return newBinding switch
		{
			PriorityBinding pb => pb,
			MultiBinding mb => mb,
			Binding b => b,
			_ => throw new InvalidOperationException("Binding could not be cast.")
		};
	}

	private static void CopyProperties<T>(T source, T result, Type type) where T : UIElement
	{
		// Copy all properties. 
		IEnumerable<PropertyInfo> properties = type.GetRuntimeProperties();
		foreach (PropertyInfo property in properties)
		{
			if (property.Name != "Name")
			{ // do not copy names or we cannot add the clone to the same parent as the original. 
				if (property.CanWrite && property.CanRead)
				{
					object? sourceProperty = property.GetValue(source);
					if (sourceProperty is UIElement element)
					{
						UIElement propertyClone = element.DeepClone();
						property.SetValue(result, propertyClone);
					}
					else
					{
						try
						{
							property.SetValue(result, sourceProperty);
						}
						catch (Exception ex)
						{
							System.Diagnostics.Debug.WriteLine(ex);
						}
					}
				}
			}
		}
	}

	public static T DeepClone<T>(this T source) where T : UIElement
	{
		T? result; // Get the type 
		Type type = source.GetType(); // Create an instance 
		result = Activator.CreateInstance(type) as T;

		if (result is null)
		{
			throw new TypeInitializationException(type.FullName, null);
		}

		CopyProperties<T>(source, result, type);
		DeepCopyChildren<T>(source, result);
		return result;
	}

	private static void DeepCopyChildren<T>(T source, T result) where T : UIElement
	{
		// Deep copy children. 
		if (source is Panel sourcePanel)
		{
			if (result is Panel resultPanel)
			{
				foreach (UIElement child in sourcePanel.Children)
				{
					// RECURSION! 
					UIElement childClone = DeepClone(child);
					resultPanel.Children.Add(childClone);
				}
			}
		}
	}

	//public static T DeepClone_Bformatter<T>(T from)
	//{
	//	using (MemoryStream s = new MemoryStream())
	//	{
	//		BinaryFormatter f = new BinaryFormatter();
	//		f.Serialize(s, from);
	//		s.Position = 0;
	//		object clone = f.Deserialize(s);
	//
	//		return (T)clone;
	//	}
	//}
}
