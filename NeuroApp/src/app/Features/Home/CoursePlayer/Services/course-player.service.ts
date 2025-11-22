import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, map } from 'rxjs';
import { environment } from '../../../../../environments/environments.development';

/**
 * Interfaces based on actual backend DTOs detected
 */
export interface CourseDetailsDto {
  id: number;
  title: string;
  description: string;
  thumbnailUrl: string;
  price: number;
  isFree: boolean;
  createdAt: string;
  categoryId: number;
  instructorId: number;
  categoryName?: string;
  instructorName?: string;
  numofModulues?: number;
  numofVideos?: number;
}

export interface ModulesDto {
  id: number;
  title: string;
  moduleArrangement: number;
  courseId: number;
  course?: any;
  videos?: VideoDto[];
}

export interface VideoDto {
  id: number;
  title: string;
  filePath: string;
  duration: number;
  videoArrangement: number;
}

export interface VideoGetAllDto {
  id: number;
  title: string;
  filePath: string;
  duration: number;
  videoArrangement: number;
  moduleId: number;
  courseId: number;
  courseName: string;
}

export interface EnrollmentsByStudentDto {
  id: number;
  enrollmentDate: string;
  progressPercentage: number;
  stdId: number;
  courseId: number;
  courseTitle: string;
  courseDescription: string;
  courseImageUrl: string;
  isDeleted: boolean;
  isCanceled: boolean;
  status: string;
  student?: any;
}

@Injectable({
  providedIn: 'root'
})
export class CoursePlayerService {
  private apiUrl: string;

  constructor(private http: HttpClient) {
    // Use environment API URL or fallback to default
    const envUrl = environment.apiUrl;
    this.apiUrl = (envUrl && typeof envUrl === 'string' && envUrl.startsWith('http'))
      ? envUrl
      : 'http://localhost:5075/api';
  }

  /**
   * Get course details by ID
   * Endpoint: GET /api/Course/{id}
   */
  getCourseById(courseId: number): Observable<CourseDetailsDto> {
    return this.http.get<CourseDetailsDto>(`${this.apiUrl}/Course/${courseId}`);
  }

  /**
   * Get all modules for a course
   * Endpoint: GET /api/Module/GetModulesByCrsID/{crsId}
   */
  getModulesByCourseId(courseId: number): Observable<ModulesDto[]> {
    return this.http.get<ModulesDto[]>(`${this.apiUrl}/Module/GetModulesByCrsID/${courseId}`);
  }

  /**
   * Get all videos
   * Endpoint: GET /api/Video/GetAllVideos
   */
  getAllVideos(): Observable<VideoGetAllDto[]> {
    return this.http.get<VideoGetAllDto[]>(`${this.apiUrl}/Video/GetAllVideos`);
  }

  /**
   * Get video by ID
   * Endpoint: GET /api/Video/GetVideoById/{id}
   */
  getVideoById(videoId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Video/GetVideoById/${videoId}`);
  }

  /**
   * Get enrollments for a student
   * Endpoint: GET /api/Enrollment/GetEnrollmentsByStudentId/{id}
   * Note: Backend expects int for studentId
   */
  getEnrollmentsByStudentId(studentId: number): Observable<EnrollmentsByStudentDto[]> {
    return this.http.get<EnrollmentsByStudentDto[]>(`${this.apiUrl}/Enrollment/GetEnrollmentsByStudentId/${studentId}`);
  }

  /**
   * Check if student is enrolled in a course
   * Returns true if enrolled, not deleted, not canceled, and status is not "Pending"
   * Based on backend Enrollment model: Status defaults to "Pending", can be "Paid", "Active", etc.
   * IsDeleted and IsCanceled are booleans
   */
  checkEnrollment(studentId: number, courseId: number): Observable<boolean> {
    return this.getEnrollmentsByStudentId(studentId).pipe(
      map(enrollments => {
        const enrollment = enrollments.find(e =>
          e.courseId === courseId &&
          !e.isDeleted &&
          !e.isCanceled &&
          e.status &&
          e.status !== 'Pending' // Allow any status except Pending (Paid, Active, Completed, etc.)
        );
        return !!enrollment;
      })
    );
  }

  /**
   * Get full course data including modules and videos
   */
  getCourseWithContent(courseId: number): Observable<{
    course: CourseDetailsDto;
    modules: ModulesDto[];
    videos: VideoGetAllDto[];
  }> {
    return forkJoin({
      course: this.getCourseById(courseId),
      modules: this.getModulesByCourseId(courseId),
      videos: this.getAllVideos()
    }).pipe(
      map(data => ({
        course: data.course,
        modules: data.modules.sort((a, b) => a.moduleArrangement - b.moduleArrangement),
        videos: data.videos.filter(v =>
          data.modules.some(m => m.id === v.moduleId)
        )
      }))
    );
  }

  /**
   * Get video URL for streaming
   * Videos are served statically from /uploads/videos/
   */
  getVideoUrl(filePath: string): string {
    if (!filePath) return '';
    // If filePath already includes full URL, return as is
    if (filePath.startsWith('http')) {
      return filePath;
    }
    // Otherwise, construct full URL
    const baseUrl = this.apiUrl.replace('/api', '');
    return `${baseUrl}${filePath.startsWith('/') ? filePath : '/' + filePath}`;
  }
}

