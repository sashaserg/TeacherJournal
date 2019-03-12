﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using TeacherJournal.database;
using TeacherJournal.model;

namespace TeacherJournal.view
{
    /// <summary>
    /// Логика взаимодействия для VocabularyWindow.xaml
    /// </summary>
    public partial class VocabularyWindow : Window
    {
        // Константы для определения отображения окна словаря
        public const int SUBJECT = 1001;
        public const int GROUP = 1002;
        public const int CLASSROOM = 1003;

        // Словарь Словарных типов( лол ).
        public Dictionary<int, String> vocabularyTypes = new Dictionary<int, string>() {
            { SUBJECT, "Словар предметів"},
            { GROUP, "Словар груп"},
            { CLASSROOM, "Словар аудиторій"}
        };

        private int currentType;
        private Term currentTerm;

        public ObservableCollection<VocabularyEntity> list;

        public VocabularyWindow()
        {
            InitializeComponent();
        }

        public VocabularyWindow(int vocabularyType, Term currentTerm)
        {
            InitializeComponent();
            this.currentType = vocabularyType;
            this.Title = vocabularyTypes[currentType];
            this.currentTerm = currentTerm;
            list = getVocabularyList();
        }

        // Обработчик загрузки окна.
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbVocabularyName.Text = vocabularyTypes[currentType];
            VocabularyGrid.ItemsSource = list;
        }

        // Обработка закрытия формы через кнопку Сохранить и закрыть.
        // Берем нашу коллекцию и передаем её в соответсвующий метод DBHelper-a.
        private void btnAcceptClose_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("list.Count: {0}", list.Count);

            if (MessageBox.Show("Ви підтверджуєте збереження нових даних?", "Підтвердження", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Список сущностей, который был при открытии окна. 
                List<VocabularyEntity> tempList = new List<VocabularyEntity>(getVocabularyList());
                // Список сущностей, которые добавили.
                List<VocabularyEntity> newItems = list.Where(c => c.id == 0).ToList();
                // Список сущностей, которые удалили
                List<VocabularyEntity> deletedItems = tempList.Where(c => !list.Any(d => c.id == d.id)).ToList();
                // Список сущностей, которые изменили
                List<VocabularyEntity> toBeUpdated = list.Where(c => tempList.Any(d => c.id == d.id)).ToList();
                try
                {
                    foreach (VocabularyEntity obj in deletedItems)
                    {
                        DBHelper.DeleteVocabularyItem(obj);
                    }
                    foreach (VocabularyEntity obj in newItems)
                    {
                        DBHelper.AddVocabularyItem(obj);
                    }
                    foreach (VocabularyEntity obj in toBeUpdated)
                    {
                        DBHelper.UpdateVocabularyItem(obj);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0} Exception caught", ex);
                }
                this.Close();
            }

        }

        // Удаляем строку в таблице удаляя объект из коллекции.
        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VocabularyEntity obj = ((FrameworkElement)sender).DataContext as VocabularyEntity;
                Console.WriteLine($"Name of deleted row: {obj.name}");
                list.Remove(obj);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Exception caught", ex);
            }
        }

        // Получить список сущностей(группы/аудитории/предметы).
        private ObservableCollection<VocabularyEntity> getVocabularyList()
        {
            ObservableCollection<VocabularyEntity> temp = null;
            if (vocabularyTypes.ContainsKey(currentType))
            {
                if (currentType == SUBJECT)
                {
                    // если словарь предметов
                    temp = new ObservableCollection<VocabularyEntity>(DBHelper.selectSubject(currentTerm));
                }
                else if (currentType == GROUP)
                {
                    // если словарь групп
                    temp = new ObservableCollection<VocabularyEntity>(DBHelper.selectGroups(currentTerm));
                }
                else if (currentType == CLASSROOM)
                {
                    // если словарь аудиторий
                    temp = new ObservableCollection<VocabularyEntity>(DBHelper.selectClassroom(currentTerm));
                }              
            }
            return temp;
        }

        // Обработка добавления новой сущности.
        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            if (currentType == SUBJECT)
            {
                // если словарь предметов
                list.Add(new Subject(this.currentTerm.id));
            }
            else if (currentType == GROUP)
            {
                // если словарь групп
                list.Add(new Group(this.currentTerm.id));
            }
            else if (currentType == CLASSROOM)
            {
                // если словарь аудиторий
                list.Add(new Classroom(this.currentTerm.id));
            }
        }
    }
    /* Два класса для сравнивания сущностей словаря
    public class IdComparer : IEqualityComparer<VocabularyEntity>
    {
        public int GetHashCode(VocabularyEntity co)
        {
            if (co == null)
            {
                return 0;
            }
            return co.id.GetHashCode();
        }

        public bool Equals(VocabularyEntity x1, VocabularyEntity x2)
        {
            if (object.ReferenceEquals(x1, x2))
            {
                return true;
            }
            if (object.ReferenceEquals(x1, null) ||
                object.ReferenceEquals(x2, null))
            {
                return false;
            }
            return x1.id == x2.id;
        }
    }

    public class NameComparer : IEqualityComparer<VocabularyEntity>
    {
        public int GetHashCode(VocabularyEntity co)
        {
            if (co == null)
            {
                return 0;
            }
            return co.name.GetHashCode();
        }

        public bool Equals(VocabularyEntity x1, VocabularyEntity x2)
        {
            if (object.ReferenceEquals(x1, x2))
            {
                return true;
            }
            if (object.ReferenceEquals(x1, null) ||
                object.ReferenceEquals(x2, null))
            {
                return false;
            }
            return x1.name == x2.name;
        }
    }*/
}
