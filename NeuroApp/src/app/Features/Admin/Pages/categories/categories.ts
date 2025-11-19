import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../Services/admin.service';
import { ICategory } from '../../Interfaces/admin.interface';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './categories.html',
  styleUrl: './categories.css'
})
export class AdminCategories implements OnInit {
  categories: ICategory[] = [];
  loading = false;

  showAddModal = false;
  showEditModal = false;
  showDeleteModal = false;
  selectedCategory: ICategory | null = null;

  newCategory: any = {
    name: '',
    description: '',
    isActive: true
  };

  constructor(private adminService: AdminService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadCategories();
    this.cdr.detectChanges();
  }

  loadCategories() {
    this.loading = true;
    this.adminService.getCategories().subscribe({
      next: (categories) => {
        this.categories = categories.map((cat: any) => ({
          id: cat.id as number,
          name: cat.name,
          description: cat.description || '',
          isActive: cat.isActive,
          coursesCount: cat.coursesCount || 0,
          createdAt: cat.createdAt || new Date()
        } as ICategory));
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.loading = false;
      }
    });
  }


  showAddCategoryModal() {
    this.newCategory = {
      name: '',
      description: '',
      isActive: true
    };
    this.showAddModal = true;
  }

  addCategory() {
    const categoryData = {
      name: this.newCategory.name,
      description: this.newCategory.description,
      isActive: this.newCategory.isActive
    };

    this.loading = true;
    this.adminService.createCategory(categoryData).subscribe({
      next: (category: any) => {
        this.loadCategories(); // Reload to get updated data
        this.showAddModal = false;
        this.loading = false;
        alert('Category created successfully');
      },
      error: (error) => {
        console.error('Error creating category:', error);
        this.loading = false;
        alert('Failed to create category: ' + (error.error?.message || error.message || 'Unknown error'));
      }
    });
  }

  editCategory(category: ICategory) {
    this.selectedCategory = { ...category };
    this.showEditModal = true;
  }

  updateCategory() {
    if (this.selectedCategory) {
      const updateData = {
        id: this.selectedCategory.id,
        name: this.selectedCategory.name,
        description: this.selectedCategory.description,
        isActive: this.selectedCategory.isActive
      };

      this.loading = true;
      this.adminService.updateCategory(this.selectedCategory.id, updateData).subscribe({
        next: (updated: any) => {
          this.loadCategories(); // Reload to get updated data
          this.showEditModal = false;
          this.loading = false;
          alert('Category updated successfully');
        },
        error: (error) => {
          console.error('Error updating category:', error);
          this.loading = false;
          alert('Failed to update category: ' + (error.error?.message || error.message || 'Unknown error'));
        }
      });
    }
  }

  confirmDelete(category: ICategory) {
    this.selectedCategory = category;
    this.showDeleteModal = true;
  }

  deleteCategory() {
    if (this.selectedCategory) {
      this.loading = true;
      this.adminService.deleteCategory(this.selectedCategory.id).subscribe({
        next: () => {
          this.loadCategories(); // Reload to get updated data
          this.showDeleteModal = false;
          this.loading = false;
          alert('Category deleted successfully');
        },
        error: (error) => {
          console.error('Error deleting category:', error);
          this.loading = false;
          alert('Failed to delete category: ' + (error.error?.message || error.message || 'Unknown error'));
        }
      });
    }
  }

  closeModals() {
    this.showAddModal = false;
    this.showEditModal = false;
    this.showDeleteModal = false;
    this.selectedCategory = null;
  }
}
