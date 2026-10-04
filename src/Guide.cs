using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace HdrPilot {
public static partial class Guide {
 sealed class Topic {public string Name,Intro;public string[] Sections;public Topic(string name,string intro,params string[] sections){Name=name;Intro=intro;Sections=sections;}}

 public static Window Create(Window owner){var topics=BeginnerTopics().Concat(NewTopics()).ToArray();StackPanel body;Window window=Dialogs.Create(owner,"Guida di HDLSS",out body);window.Width=960;window.MaxWidth=SystemParameters.WorkArea.Width-40;window.MaxHeight=SystemParameters.WorkArea.Height-40;var layout=new Grid{Height=Math.Max(380,Math.Min(570,SystemParameters.WorkArea.Height-170)),Margin=new Thickness(0,12,0,0)};layout.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(215)});layout.ColumnDefinitions.Add(new ColumnDefinition());var menu=new StackPanel{Margin=new Thickness(0,0,18,0)};var menuScroll=new ScrollViewer{Content=menu,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};layout.Children.Add(menuScroll);var content=new StackPanel{Margin=new Thickness(22,0,18,12)};var scroll=new ScrollViewer{Content=content,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};Grid.SetColumn(scroll,1);layout.Children.Add(scroll);body.Children.Add(layout);var buttons=new List<Button>();Action<int> select=index=>{content.Children.Clear();for(int i=0;i<buttons.Count;i++){buttons[i].Background=new SolidColorBrush((Color)ColorConverter.ConvertFromString(i==index?"#41331E":"#1B222C"));buttons[i].Foreground=new SolidColorBrush((Color)ColorConverter.ConvertFromString(i==index?"#E6AC50":"#BBC4D0"));}var topic=topics[index];var heading=Dialogs.Text(topic.Name,24);heading.Foreground=new SolidColorBrush(Color.FromRgb(230,172,80));content.Children.Add(heading);content.Children.Add(Dialogs.Text(topic.Intro,13));for(int n=0;n<topic.Sections.Length;n+=2){var h=Dialogs.Text(topic.Sections[n],15);h.Foreground=new SolidColorBrush(Color.FromRgb(230,172,80));h.Margin=new Thickness(0,14,0,8);content.Children.Add(h);content.Children.Add(Dialogs.Text(topic.Sections[n+1],13));}scroll.ScrollToTop();};for(int i=0;i<topics.Length;i++){int index=i;var button=new Button{Content=new TextBlock{Text=topics[i].Name,TextWrapping=TextWrapping.Wrap},HorizontalContentAlignment=HorizontalAlignment.Left,Padding=new Thickness(10,12,10,12),Margin=new Thickness(0,0,0,7),FontSize=12};button.Click+=(s,e)=>select(index);buttons.Add(button);menu.Children.Add(button);}select(0);return window;}
 public static void Show(Window owner){Create(owner).ShowDialog();}
}
}

