<!-- default file list -->
*Files to look at*:

* [CRUDBehaviorBase.cs](./CS/CRUDBehavior/CRUDBehaviorBase.cs) (VB: [CRUDBehaviorBase.vb](./VB/CRUDBehavior/CRUDBehaviorBase.vb))
* [XPOInstantModeCRUDBehavior.cs](./CS/CRUDBehavior/XPOInstantModeCRUDBehavior.cs) (VB: [XPOInstantModeCRUDBehavior.vb](./VB/CRUDBehavior/XPOInstantModeCRUDBehavior.vb))
* [XPOServerModeCRUDBehavior.cs](./CS/CRUDBehavior/XPOServerModeCRUDBehavior.cs) (VB: [XPOServerModeCRUDBehavior.vb](./VB/CRUDBehavior/XPOServerModeCRUDBehavior.vb))
* **[MainWindow.xaml](./CS/XPOInstant/MainWindow.xaml) (VB: [MainWindow.xaml](./VB/XPOInstant/MainWindow.xaml))**
* [MainWindow.xaml.cs](./CS/XPOInstant/MainWindow.xaml.cs) (VB: [MainWindow.xaml](./VB/XPOInstant/MainWindow.xaml))
* [MainWindow.xaml](./CS/XPOServer/MainWindow.xaml) (VB: [MainWindow.xaml.vb](./VB/XPOServer/MainWindow.xaml.vb))
* [MainWindow.xaml.cs](./CS/XPOServer/MainWindow.xaml.cs) (VB: [MainWindow.xaml.vb](./VB/XPOServer/MainWindow.xaml.vb))
<!-- default file list end -->
# How to implement CRUD operations using DXGrid and eXpress Persistent Objects


<p>This example shows how to use XPInstantFeedbackSource or XPServerCollectionSource with DXGrid, and how to implement CRUD operations (e.g., add, remove, edit) in your application via special behavior.</p>
<p>The test sample requires the SQL Express service and MSAccess to be installed on your machine.</p>
<p>We have created the XPOServerModeCRUDBehavior and XPOInstantModeCRUDBehavior attached behaviors for GridControl. For instance:</p>


```xml
<dxg:GridControl><br>
   <i:Interaction.Behaviors><br>
       <crud:XPOServerModeCRUDBehavior .../><br>
   </i:Interaction.Behaviors><br>
</dxg:GridControl>
```


<p> </p>
<p>The XPServerModeCRUDBehavior and XPInstantModeCRUDBehavior classes contain the NewRowForm and EditRowForm properties to provide the "Add Row" and "Edit Row" actions. With these properties, you can create the Add and Edit forms according to your requirements:</p>


```xml
<DataTemplate x:Key="EditRecordTemplate"><br>
   <StackPanel Margin="8" MinWidth="200"><br>
       <Grid><br>
           <Grid.ColumnDefinitions><br>
               <ColumnDefinition/><br>
               <ColumnDefinition/><br>
           </Grid.ColumnDefinitions><br>
           <Grid.RowDefinitions><br>
               <RowDefinition/><br>
               <RowDefinition/><br>
           </Grid.RowDefinitions><br>
           <TextBlock Text="ID:" VerticalAlignment="Center" Grid.Row="0" Grid.Column="0" Margin="0,0,6,4" /><br>
           <dxe:TextEdit x:Name="txtID" Grid.Row="0" Grid.Column="1" EditValue="{Binding Path=Id, Mode=TwoWay}" Margin="0,0,0,4" /><br>
           <TextBlock Text="Name:" VerticalAlignment="Center" Grid.Row="1" Grid.Column="0" Margin="0,0,6,4" /><br>
           <dxe:TextEdit x:Name="txtCompany" Grid.Row="1" Grid.Column="1" EditValue="{Binding Path=Name, Mode=TwoWay}" Margin="0,0,0,4" /><br>
       </Grid><br>
   </StackPanel><br>
</DataTemplate><br>
<crud:XPServerModeCRUDBehavior NewRowForm="{StaticResource ResourceKey=EditRecordTemplate}" EditRowForm="{StaticResource ResourceKey=EditRecordTemplate}"/> <br>

```


<p>This Behavior classes requires the following information from your data model:</p>
<p>- XPObjectType - the type of rows;</p>
<p>- DataServiceContext - database entities;</p>
<p>- PrimaryKey - the primary key of the database table;</p>
<p>- CollectionSource/InstantCollectionSource - an object of the EntityServerModeDataSource type.</p>


```xml
<dxg:GridControl><br>
   <i:Interaction.Behaviors><br>
       <crud:XPOServerModeCRUDBehavior XPObjectType="{x:Type local:Items}" CollectionSource="{Binding Collection}" PrimaryKey="Id"/><br>
   </i:Interaction.Behaviors><br>
</dxg:GridControl><br>

```


<p>The XPInstantModeCRUDBehavior class for SL contains the ServiceHelper property, which refers to an object that provides actions to work with databases</p>


```cs
helper.ServiceHelper = new ServiceHelper(helper, new Uri("<a href='http://localhost'>http://localhost</a>:54177/WcfDataService.svc/"));<br>

```


<p>See the <a href="http://documentation.devexpress.com/#XPO/clsDevExpressXpoXPInstantFeedbackSourcetopic"><u>XPInstantFeedbackSource</u></a> and <a href="http://documentation.devexpress.com/#XPO/clsDevExpressXpoXPServerCollectionSourcetopic"><u>XPServerCollectionSource</u></a> classes to learn more about XPInstantFeedbackSource and XPServerCollectionSource.</p>
<p>Behavior class descendants support the following commands: NewRowCommand, RemoveRowCommand, EditRowCommand. You can bind your interaction controls with these commands with ease. For instance:</p>


```xml
<crud:XPOServerModeCRUDBehavior x:Name="helper"/><br>
<StackPanel Grid.Row="1" Orientation="Horizontal" HorizontalAlignment="Center"><br>
   <Button Height="22" Width="60" Command="{Binding Path=NewRowCommand, ElementName=helper}">Add</Button><br>
   <Button Height="22" Width="60" Command="{Binding Path=RemoveRowCommand, ElementName=helper}" Margin="6,0,6,0">Remove</Button><br>
   <Button Height="22" Width="60" Command="{Binding Path=EditRowCommand, ElementName=helper}">Edit</Button><br>
</StackPanel><br>
<br>
```


<p>By default, the XPOServerModeCRUDBehavior and XPOInstantModeCRUDBehavior solution support the following end-user interaction capabilities:</p>
<p>1. An end-user can edit selected row values by double-clicking on a grid row or by pressing the Enter key if the AllowKeyDownActions property is True.</p>
<p>2. An end-user can remove selected rows via the Delete key press if the AllowKeyDownActions property is True.</p>
<p>3. An end-user can add new rows, remove and edit them via the NewRowCommand, RemoveRowCommand, and EditRowCommand commands.<br /><br /><br />To learn more on how to implement similar functionality in <strong>Silverlight,</strong> refer to the <a href="https://www.devexpress.com/Support/Center/p/T245411">T245411</a> example.</p>

<br/>


