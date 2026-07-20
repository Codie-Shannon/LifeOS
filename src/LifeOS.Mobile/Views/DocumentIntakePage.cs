using LifeOS.Mobile.Core.Documents; using LifeOS.Core.Documents;
namespace LifeOS.Mobile.Views;
public sealed class DocumentIntakePage:ContentPage
{
 readonly V11DocumentIntakeService service=new(); readonly VerticalStackLayout stack=new(){Padding=18,Spacing=12};
 public DocumentIntakePage(){Title="Documents";BackgroundColor=Color.FromArgb("#101017");Content=new ScrollView{Content=stack};}
 protected override void OnAppearing(){base.OnAppearing();Render();}
 void Render(){var (r,d)=service.BuildProofData(DateTimeOffset.Now);stack.Children.Clear();stack.Children.Add(new Label{Text="Document intake",FontSize=32,FontAttributes=FontAttributes.Bold,TextColor=Colors.White});stack.Children.Add(new Label{Text="Capture • review • preserve originals",TextColor=Color.FromArgb("#B8C5DE")});Add("Capture draft",r.Single(x=>x.Id=="doc-mobile-timesheet"),"Draft retained locally until deliberate review and upload.");Add("Extraction suggestions",r.Single(x=>x.State==DocumentIntakeState.ReviewRequired),"Suggestions remain untrusted.");Add("Exact-hash duplicate",r.Single(x=>x.Id=="doc-receipt-copy"),$"Candidate only • {d.Count} exact match • no automatic merge.");Add("Accepted and linked",r.Single(x=>x.State==DocumentIntakeState.Accepted),"Linked to Money and Work after explicit review.");}
 void Add(string heading,DocumentRecord x,string note){var b=new Button{Text=$"{heading}\n{x.Original.FileName} • {x.State}\n{note}",TextColor=Colors.White,BackgroundColor=Color.FromArgb("#1A2030"),HorizontalOptions=LayoutOptions.Fill,Padding=14};stack.Children.Add(b);}
}
