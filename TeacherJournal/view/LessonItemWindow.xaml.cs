﻿using System;
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
using System.Windows.Shapes;
using TeacherJournal.database;
using TeacherJournal.model;
using TeacherJournal.view;

namespace TeacherJournal.view
{
    /// <summary>
    /// Логика взаимодействия для LessonItemWindow.xaml
    /// </summary>
    public partial class LessonItemWindow : Window
    {
        private MainWindow mainWindow;
        private Lesson currentLesson;
        private Term currentTerm;
        private List<Group> groups;
        private List<Classroom> classrooms;
        private List<Subject> subjects;
        private List<TypeOfLesson> typesOfLesson;

        private int[] numsOfLessons = new int[8] { 1, 2, 3, 4, 5, 6, 7, 8};
        

        public LessonItemWindow()
        {
            InitializeComponent();
        }

        public LessonItemWindow(Lesson lesson, Term term, MainWindow mainWindow)
        {
            InitializeComponent();

            this.currentLesson = lesson;
            this.currentTerm = term;
            this.mainWindow = mainWindow;

            try
            {
                this.subjects = new List<Subject>(DBHelper.selectSubject(currentTerm));
                this.subjects.Sort((p, q) => p.name.CompareTo(q.name));
                this.classrooms = new List<Classroom>(DBHelper.selectClassroom(currentTerm));
                this.classrooms.Sort((p, q) => p.name.CompareTo(q.name));
                this.groups = new List<Group>(DBHelper.selectGroups(currentTerm));
                this.groups.Sort((p, q) => p.name.CompareTo(q.name));
                this.typesOfLesson = new List<TypeOfLesson>(DBHelper.selectTypesOfLesson());
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Exception caught", ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbLessonSubject.ItemsSource = subjects;
            cbLessonClassroom.ItemsSource = classrooms;
            cbLessonType.ItemsSource = typesOfLesson;
            cbLessonNumber.ItemsSource = numsOfLessons;
            // Если редактируем занятие.
            if (currentLesson != null)
            {
                foreach (Subject item in cbLessonSubject.Items)
                {
                    if (item.id == currentLesson.subject.id)
                    {
                        cbLessonSubject.SelectedItem = item;
                        break;
                    }
                }
                foreach (Classroom item in cbLessonClassroom.Items)
                {
                    if (item.id == currentLesson.classroom.id)
                    {
                        cbLessonClassroom.SelectedItem = item;
                        break;
                    }
                }
                foreach (TypeOfLesson item in cbLessonType.Items)
                {
                    if (item.id == currentLesson.typeOfLesson.id)
                    {
                        cbLessonType.SelectedItem = item;
                        break;
                    }
                }
                foreach (Group group in currentLesson.groups)
                {
                    AddNewGroup(group);
                }
                tbLessonTheme.Text = currentLesson.theme;
                cbLessonNumber.SelectedItem = cbLessonNumber.Items[currentLesson.numOfLesson - 1];
                dpLessonDate.SelectedDate = currentLesson.date.Date;
            }
            // Если создаем занятие.
            else
            {
                AddNewGroup();
            }
        }

        // Добавляем новую группу, если передаем аргументом группу - выставляем её как SelectedItem.
        private void AddNewGroup(Group group = null)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;

            ComboBox cbGroup = new ComboBox();
            cbGroup.Width = 130;
            cbGroup.ItemsSource = groups;
            cbGroup.DisplayMemberPath = "name";
            cbGroup.SelectedValuePath = "id";

            Button btn = new Button();
            btn.Content = (Image)FindResource("TrashBoxImage");
            btn.Margin = new Thickness(5, 0, 0, 0);
            btn.Click += new RoutedEventHandler(btnDeleteGroup_Click);
            if (group != null)
            {
                foreach (Group item in cbGroup.Items)
                {
                    if (item.id == group.id)
                    {
                        cbGroup.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                if (cbGroup.Items.Count > 0)
                    cbGroup.SelectedItem = cbGroup.Items[0];
            }

            stackPanel.Children.Add(cbGroup);
            stackPanel.Children.Add(btn);

            GroupVerticalPanel.Children.Add(stackPanel);
        }

        // Ивент нажатия хз зачем он тут вообще нужен, пока что подержу а там посмотрим.
        private void btnAddNewGroup_Click(object sender, RoutedEventArgs e)
        {
            AddNewGroup();
        }

        // Удалить группу.
        private void btnDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            StackPanel stackPanel = (StackPanel)((Button)sender).Parent;
            GroupVerticalPanel.Children.Remove(stackPanel);
        }

        private void AcceptAndSave(object sender, RoutedEventArgs e)
        {
            if ((dpLessonDate.SelectedDate != null) && (cbLessonSubject.SelectedItem != null) && (cbLessonClassroom.SelectedItem != null)
                && AreGroupsFilled() && (cbLessonNumber.SelectedItem != null))
            {
                
                Lesson lesson = new Lesson();
                lesson.numOfLesson = (Int32)cbLessonNumber.SelectedValue;
                lesson.subject = cbLessonSubject.SelectedItem as Subject;
                lesson.classroom = cbLessonClassroom.SelectedItem as Classroom;
                lesson.theme = tbLessonTheme.Text;
                lesson.idTerm = currentTerm.id;
                lesson.date = dpLessonDate.SelectedDate.Value.Date;
                lesson.typeOfLesson = cbLessonType.SelectedItem as TypeOfLesson;
                lesson.groups = new List<Group>();

                //Устанавливаем количество часов
                if((bool)cbCountOfHours.IsChecked)
                {
                    lesson.countOfHours = 2;
                }else
                {
                    lesson.countOfHours = 1;
                }

                // Проходим по всем комбобоксам групп и добавряем выбранные группы в groups.
                foreach (StackPanel child in GroupVerticalPanel.Children)
                {
                    foreach (object _child in child.Children)
                    {
                        if (_child.GetType().Name == "ComboBox")
                        {
                            Group group = ((ComboBox)_child).SelectedItem as Group;
                            lesson.groups.Add(group);
                            break;
                        }
                    }
                }

                var list = this.mainWindow.lessonList;
                if (currentLesson == null)//если мы добавляем новое занятие
                {
                    DBHelper.addLesson(lesson);
                }
                else //если мы изменяем существующее занятие
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list.ElementAt(i).id == currentLesson.id)
                        {
                            list[i] = lesson;
                        }
                    }
                    DBHelper.deleteLesson(currentLesson);
                    DBHelper.addLesson(lesson);
                }
                    
                this.Close();
            }
            else
            {
                MessageBox.Show("Введіть правильний номер заняття!", "Попередження!");
            }
            
               
        }

        private bool AreGroupsFilled()
        {
            if (GroupVerticalPanel.Children.Count != 0)
            {
                foreach (StackPanel child in GroupVerticalPanel.Children)
                {
                    foreach (object _child in child.Children)
                    {
                        if (_child.GetType().Name == "ComboBox")
                        {
                            ComboBox cb = (ComboBox)_child;
                            if (cb.SelectedItem == null)
                                return false;
                            break;
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
