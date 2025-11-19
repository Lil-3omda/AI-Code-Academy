import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../Services/admin.service';
import { IAnalytics } from '../../Interfaces/admin.interface';

@Component({
  selector: 'app-admin-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analytics.html',
  styleUrl: './analytics.css'
})
export class AdminAnalytics implements OnInit {
  analytics: IAnalytics | null = null;
  loading = false;

  constructor(private adminService: AdminService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadAnalytics();
    this.cdr.detectChanges();
  }

  loadAnalytics() {
    this.loading = true;
    this.adminService.getAnalytics().subscribe({
      next: (data) => {
        this.analytics = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading analytics:', error);
        // Set empty analytics instead of mock data
        this.analytics = {
          totalStudents: 0,
          totalCourses: 0,
          totalEnrollments: 0,
          totalInstructors: 0,
          activeStudents: 0,
          pendingInstructors: 0,
          totalCategories: 0,
          recentEnrollments: [],
          enrollmentsByMonth: [],
          coursesByCategory: [],
          topInstructors: []
        };
        this.loading = false;
        this.cdr.detectChanges();
        // Show error message to user
        alert('Failed to load analytics data. Please try again later.');
      }
    });
  }

  getMaxEnrollments(): number {
    if (!this.analytics?.enrollmentsByMonth || this.analytics.enrollmentsByMonth.length === 0) return 0;
    return Math.max(...this.analytics.enrollmentsByMonth.map(e => e.enrollments));
  }

  getEnrollmentPercentage(enrollments: number): number {
    const max = this.getMaxEnrollments();
    return max > 0 ? (enrollments / max) * 100 : 0;
  }

  getTotalCoursesCount(): number {
    if (!this.analytics?.coursesByCategory) return 0;
    return this.analytics.coursesByCategory.reduce((sum, cat) => sum + cat.coursesCount, 0);
  }

  getCategoryPercentage(count: number): number {
    const total = this.getTotalCoursesCount();
    return total > 0 ? (count / total) * 100 : 0;
  }

  getCategoryColor(index: number): string {
    const colors = ['#4361ee', '#10b981', '#f72585', '#f59e0b', '#8b5cf6', '#06b6d4'];
    return colors[index % colors.length];
  }
  get completionRate(): number {
    if (!this.analytics || this.analytics.totalEnrollments === 0) return 0;

    const completed = this.analytics.recentEnrollments
      ?.filter(e => e?.status === 'Completed')?.length || 0;

    return (completed / this.analytics.totalEnrollments) * 100;
  }

}
