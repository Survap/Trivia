using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TShockAPI;

namespace Trivia
{
    public class TriviaManager
    {
        public Config Config;
        public bool PendingAnswer { get { return QuestionAsked; } }
        private Timer _timer = new Timer(1000);
        private QAndA CurrentQandA;
        public List<string> WrongAnswers = new List<string>();

        public bool Enabled
        {
            get
            {
                return _timer.Enabled;
            }
            set
            {
                _timer.Enabled = value;
            }
        }

        public TriviaManager()
        {
            _timer.Elapsed += _timer_Elapsed;
        }

        ~TriviaManager()
        {
            _timer.Elapsed -= _timer_Elapsed;
        }

        public void Start()
        {
            this.Enabled = true;
        }

        public void Stop()
        {
            this.Enabled = false;
        }

        public void Initialize()
        {
            Load_Config();
        }

        public void Load_Config()
        {
            Config = new Config(Config.SavePath);
            this.Enabled = Config.Enabled;
        }

        public void ReloadConfig(CommandArgs args)
        {
            Config.Reload(args);
        }

        private void Reset()
        {
            count = 0;
            QuestionAsked = false;
        }

        private int count = 0;
        private bool QuestionAsked;
        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            count++;
            if (count > Config.QuestionInterval && !QuestionAsked)
            {
                count = 0;
                QuestionAsked = true;
                SetNewQandA();
                AnnouceQuestion();
            }
            if (count > Config.AnswerTime && QuestionAsked)
            {
                count = 0;
                QuestionAsked = false;
                TSPlayer.All.SendErrorMessage("[Trivia] Time's up!");
                EndTriviaNoAnswers();
            }
        }

        public bool IsAnswerCorrect(string answer)
        {
            return CurrentQandA.Answers.Any(a => a.Equals(answer, StringComparison.CurrentCultureIgnoreCase));
        }

        private void AnnouceQuestion()
        {
            TSPlayer.All.SendInfoMessage("[Trivia] Here comes a trivia question! Use /answer <ANSWER> or /a <ANSWER> to join.");
            TSPlayer.All.SendInfoMessage("[Trivia] " + CurrentQandA.Question);
        }

        private void SetNewQandA()
        {
            Random rnd = new Random();
            CurrentQandA = Config.QuestionsAndAnswers[rnd.Next(0, Config.QuestionsAndAnswers.Length)];
        }

        public void EndTrivia(TSPlayer ts)
        {
            Reset();
            TSPlayer.All.SendInfoMessage(string.Format("[Trivia] {0} answered the trivia correctly! the answer{1} {2}", ts.Name, CurrentQandA.Answers.Count > 1 ? "s were" : " was", string.Join(", ", CurrentQandA.Answers)));
            if (Config.DisplayWrongAnswers && WrongAnswers.Count > 0)
                TSPlayer.All.SendErrorMessage(string.Format("[Trivia] Wrong answers were: {0}", string.Join(", ", WrongAnswers)));
            WrongAnswers.Clear();
        }

        public void EndTriviaNoAnswers()
        {
            Reset();
            TSPlayer.All.SendInfoMessage(string.Format("[Trivia] No one answered the trivia correctly :( the answer{0} {1}", 
                CurrentQandA.Answers.Count > 1 ? "s were" : " was", 
                string.Join(", ", CurrentQandA.Answers)));

            if (Config.DisplayWrongAnswers && WrongAnswers.Count > 0)
                TSPlayer.All.SendErrorMessage(string.Format("[Trivia] Wrong answers were: {0}", string.Join(", ", WrongAnswers)));
            WrongAnswers.Clear();
        }
    }
}
