import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../Services/admin.service';
import { ICourse, ICategory, IInstructor } from '../../Interfaces/admin.interface';

@Component({
  selector: 'app-admin-courses',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './courses.html',
  styleUrl: './courses.css'
})
export class AdminCourses implements OnInit {
  courses: ICourse[] = [];
  categories: ICategory[] = [];
  instructors: IInstructor[] = [];
  loading = true;
  searchTerm = '';
  showAddModal = false;
  showEditModal = false;
  showDeleteModal = false;
  selectedCourse: ICourse | null = null;

  newCourse: any = {
    title: '',
    description: '',
    thumbnailUrl: '',
    price: 0,
    isFree: true,
    instructorId: 0,
    categoryId: 0
  };

  thumbnailFile: File | null = null;
  editThumbnailFile: File | null = null;

  constructor(private adminService: AdminService, private cdr: ChangeDetectorRef  ) {}

  ngOnInit() {
    this.loadCourses();
    this.loadCategories();
    this.loadInstructors();
    this.cdr.detectChanges();
  }

  loadCourses() {
    this.loading = true;
    this.adminService.getCourses().subscribe({
      next: (data) => {
        this.courses = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading courses:', error);
        this.loading = false;
        this.showErrorMessage('Failed to load courses');
      }
    });
  }

  loadCategories() {
    this.adminService.getCategories().subscribe({
      next: (data) => {
        this.categories = data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  loadInstructors() {
    this.adminService.getInstructors().subscribe({
      next: (data) => {
        this.instructors = data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading instructors:', error);
      }
    });
  }

  showAddCourseModal() {
    this.newCourse = {
      title: '',
      description: '',
      thumbnailUrl: '',
      price: 0,
      isFree: true,
      instructorId: 0,
      categoryId: 0
    };
    this.thumbnailFile = null;
    this.showAddModal = true;
  }

  addCourse() {
    if (!this.newCourse.title || !this.newCourse.description || !this.newCourse.instructorId || !this.newCourse.categoryId) {
      this.showErrorMessage('Please fill in all required fields');
      return;
    }

    this.loading = true;
    this.adminService.createCourse(this.newCourse, this.thumbnailFile || undefined).subscribe({
      next: (course) => {
        this.loadCourses(); // Reload to get updated data
        this.showAddModal = false;
        this.thumbnailFile = null;
        this.loading = false;
        this.showSuccessMessage('Course created successfully');
      },
      error: (error) => {
        console.error('Error creating course:', error);
        this.loading = false;
        this.showErrorMessage('Failed to create course: ' + (error.error?.message || error.message || 'Unknown error'));
      }
    });
  }

  editCourse(course: ICourse) {
    this.selectedCourse = { ...course };
    this.editThumbnailFile = null;
    this.showEditModal = true;
  }

 updateCourse() {
  if (this.selectedCourse?.id != null) {
    this.loading = true;
    this.adminService.updateCourse(this.selectedCourse.id, this.selectedCourse, this.editThumbnailFile || undefined).subscribe({
      next: () => {
        this.loadCourses(); // Reload to get updated data
        this.showEditModal = false;
        this.editThumbnailFile = null;
        this.loading = false;
        this.showSuccessMessage('Course updated successfully');
      },
      error: (error) => {
        console.error('Error updating course:', error);
        this.loading = false;
        this.showErrorMessage('Failed to update course: ' + (error.error?.message || error.message || 'Unknown error'));
      }
    });
  } else {
    this.showErrorMessage('Invalid course ID for update.');
  }
}

  deleteCourse() {
  if (this.selectedCourse?.id != null) {
    this.loading = true;
    this.adminService.deleteCourse(this.selectedCourse.id).subscribe({
      next: () => {
        this.loadCourses(); // Reload to get updated data
        this.showDeleteModal = false;
        this.loading = false;
        this.showSuccessMessage('Course deleted successfully');
      },
      error: (error) => {
        console.error('Error deleting course:', error);
        this.loading = false;
        this.showErrorMessage('Failed to delete course: ' + (error.error?.message || error.message || 'Unknown error'));
      }
    });
  } else {
    this.showErrorMessage('Invalid course ID for delete.');
  }
}


  confirmDelete(course: ICourse) {
    this.selectedCourse = course;
    this.showDeleteModal = true;
  }



  getFilteredCourses() {
    if (!this.searchTerm) {
      return this.courses;
    }
    return this.courses.filter(course =>
      course.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      course.description?.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      course.instructorName?.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      course.categoryName?.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }

  onSearch(event: Event) {
    const input = event.target as HTMLInputElement;
    this.searchTerm = input.value;
  }

  getCategoryName(categoryId: number): string {
    const category = this.categories.find(c => c.id === categoryId);
    return category ? category.name : 'Unknown Category';
  }

  getInstructorName(instructorId: number): string {
    const instructor = this.instructors.find(i => i.id === instructorId);
    return instructor ? instructor.expertise : 'Unknown Instructor';
  }

  onThumbnailSelected(event: Event, isEdit: boolean = false) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      if (isEdit) {
        this.editThumbnailFile = input.files[0];
      } else {
        this.thumbnailFile = input.files[0];
      }
    }
  }

  closeModals() {
    this.showAddModal = false;
    this.showEditModal = false;
    this.showDeleteModal = false;
    this.selectedCourse = null;
    this.thumbnailFile = null;
    this.editThumbnailFile = null;
  }

  private showSuccessMessage(message: string) {
    // Using alert for now, can be replaced with toast notifications
    alert(message);
  }

  private showErrorMessage(message: string) {
    // Using alert for now, can be replaced with toast notifications
    alert(message);
  }
}
