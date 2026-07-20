namespace LifeOS.Mobile.Views;
public sealed class FinancialListPage : ContentPage
{
 public FinancialListPage(string title,IEnumerable<string> rows){Title=title;BackgroundColor=MoneyVisuals.Background;var c=new VerticalStackLayout{Padding=20,Spacing=14};c.Children.Add(new Label{Text=title,FontSize=30,FontAttributes=FontAttributes.Bold,TextColor=Colors.White});foreach(var row in rows)c.Children.Add(MoneyVisuals.CardView("Record",row));Content=new ScrollView{Content=c};}
}
