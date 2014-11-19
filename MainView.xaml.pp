<Window x:Class="$rootnamespace$.Views.MainView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="MainWindow" MinHeight="300" MinWidth="400"
        DataContext="{Binding Main, Source={StaticResource Locator}}">
    <Window.Resources>
        <DataTemplate x:Key="ItemHeaderTemplate">
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="{Binding Name}"/>
            </StackPanel>
        </DataTemplate>
    </Window.Resources>
    <Grid Margin="10">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="4*"/>
            <ColumnDefinition Width="3*"/>
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition/>
            </Grid.RowDefinitions>
            <StackPanel>
                <TextBlock Text="Items:" Margin="10,10,10,0" Foreground="#FF8F8F8F"/>
            </StackPanel>
            <ListBox ItemsSource="{Binding Items}" 
                     ItemTemplate="{DynamicResource ItemHeaderTemplate}" 
                     SelectedItem="{Binding SelectedItem}"
                     Margin="10" Grid.Row="1" />
        </Grid>
        <StackPanel DataContext="{Binding ItemDetail}" 
                    HorizontalAlignment="Stretch" 
                    VerticalAlignment="Top" 
                    Margin="10" Grid.Row="0" Grid.Column="1">
            <TextBlock TextWrapping="Wrap" Text="Name:" Margin="10,0,10,0" Foreground="#FF8F8F8F"/>
            <TextBox Height="23" TextWrapping="Wrap" Text="{Binding Name}" Margin="10"/>
        </StackPanel>
        <StackPanel VerticalAlignment="Bottom" Orientation="Horizontal" 
                    Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="2">
            <Button Content="New" Width="75" Margin="10" Height="25" Command="{Binding AddItem}"/>
            <Button Content="Delete" Width="75" Margin="10" Height="25" Command="{Binding DeleteItem}"/>
            <Button Content="Up" Width="75" Margin="10" Height="25" Command="{Binding MoveItemUp}"/>
            <Button Content="Down" Width="75" Margin="10" Height="25" Command="{Binding MoveItemDown}"/>
        </StackPanel>
    </Grid>
</Window>
