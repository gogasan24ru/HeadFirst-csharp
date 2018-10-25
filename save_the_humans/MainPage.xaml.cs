using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace save_the_humans
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private Random random = new Random();
        private DispatcherTimer enemyTimer = new DispatcherTimer();
        private DispatcherTimer targeTimer = new DispatcherTimer();
        private bool humanCaptured = false;

        public MainPage()
        {
            InitializeComponent();
            enemyTimer.Tick += enemyTimer_Tick;

            targeTimer.Tick += targeTimer_Tick;
            targeTimer.Interval=TimeSpan.FromSeconds(1);
        }

        private void targeTimer_Tick(object sender, EventArgs e)
        {
            progressBar.Value += 1;
            if (progressBar.Value >= progressBar.Maximum)
                EndTheGame();
        }

        private void EndTheGame()
        {
            enemyTimer.Stop();
            targeTimer.Stop();
            humanCaptured = false;
            SetupGroup.Visibility = Visibility.Visible;
            playArea.Children.Add(gameOverText);
        }

        private void enemyTimer_Tick(object sender, EventArgs e)
        {
            AddEnemy();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            Human.IsHitTestVisible = true;
            humanCaptured = false;
            progressBar.Value = 0;
            SetupGroup.Visibility = Visibility.Collapsed;
            playArea.Children.Clear();
            playArea.Children.Add(Human);
            playArea.Children.Add(Target);
            enemyTimer.Start();
            targeTimer.Start();
            gameOverText.Text = "GAME OVER!";
            enemyTimer.Interval = TimeSpan.FromSeconds(EnemySpawnIntervalSlider.Value);

        }

        private void AddEnemy()
        {
            ContentControl enemy = new ContentControl();
            enemy.Template = Resources["EnemyTemplate"] as ControlTemplate;
            AnimateEnemy(enemy,0,playArea.ActualWidth-100,"(Canvas.Left)");
            AnimateEnemy(enemy, random.Next((int) playArea.ActualHeight - 100),
                random.Next((int)playArea.ActualHeight - 100), "(Canvas.Top)");
            playArea.Children.Add(enemy);

            enemy.MouseEnter += enemy_PointerEntered;
        }

        private void enemy_PointerEntered(object sender, MouseEventArgs e)
        {
            if(humanCaptured)EndTheGame();
        }

        private void AnimateEnemy(ContentControl enemy, double from, double to, string propertyToAnimate)
        {
            Storyboard storyboard = new Storyboard() { AutoReverse=true, RepeatBehavior=RepeatBehavior.Forever};
            DoubleAnimation animation = new DoubleAnimation()
            {
                From=from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(random.Next(4,6)))
            };
            Storyboard.SetTarget(animation,enemy);
            Storyboard.SetTargetProperty(animation,new PropertyPath(propertyToAnimate));
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void Human_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (enemyTimer.IsEnabled)
            {
                humanCaptured = true;
                Human.IsHitTestVisible = false;
            }
        }

        private void Target_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (humanCaptured && targeTimer.IsEnabled)
            {
                progressBar.Value = 0;
                humanCaptured = false;
                Human.IsHitTestVisible = false;
                gameOverText.Text = "Humans survived!!";
                EndTheGame();
            }
        }

        private void PlayArea_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (humanCaptured)
            {
                Point pointerPosition = e.GetPosition(null);
                Point relativePosition = grid.TransformToVisual(playArea).Transform(pointerPosition);
                if (
                    (Math.Abs(relativePosition.X - Canvas.GetLeft(Human)) > Human.ActualWidth * 3)
                    ||
                    (Math.Abs(relativePosition.Y - Canvas.GetTop(Human))>Human.ActualHeight*3)
                    )
                {
                    humanCaptured = false;
                    Human.IsHitTestVisible = true;
                }

                else
                {
                    Canvas.SetLeft(Human, relativePosition.X - Human.ActualWidth / 2);
                    Canvas.SetTop(Human, relativePosition.Y - Human.ActualHeight / 2);
                }
            }
        }

        private void PlayArea_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if(humanCaptured)EndTheGame();
        }

        private void EnemySpawnIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EnemySpawnIntervalText.Text = "Spawn interval: "+EnemySpawnIntervalSlider.Value.ToString("N2") + "s";
        }

        private void GameTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GameTimeText.Text = "Game time: " + GameTimeSlider.Value.ToString("N2") + "s";
            if (progressBar != null) progressBar.Maximum = GameTimeSlider.Value;
        }
    }
}
