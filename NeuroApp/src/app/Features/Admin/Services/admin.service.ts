import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, forkJoin, map } from 'rxjs';
import { environment } from '../../../../environments/environments.development';
import {
  ICourse,
  ICourseCreate,
  ICourseUpdate,
  IInstructor,
  IInstructorRegister,
  IEnrollment,
  IEnrollmentCreate,
  IEnrollmentUpdate,
  IModule,
  IModuleCreate,
  IModuleUpdate,
  IVideo,
  IVideoUpdate,
  ICategory,
  ICategoryCreate,
  ICategoryUpdate,
  IAnalytics,
  IDashboardStats
} from '../../Admin/Interfaces/admin.interface';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl: string;

  constructor(@Inject(HttpClient) private http: HttpClient) {
    this.apiUrl = (typeof environment.apiUrl === 'string' && environment.apiUrl.startsWith('http'))
      ? environment.apiUrl
      : 'http://localhost:5075/api';
    console.log('[AdminService] apiUrl =', this.apiUrl);
  }

  // ==================== DASHBOARD & ANALYTICS ====================

  getDashboardStats(): Observable<IDashboardStats> {
    return forkJoin({
      courses: this.http.get<ICourse[]>(`${this.apiUrl}/Course`),
      instructors: this.http.get<IInstructor[]>(`${this.apiUrl}/Instructor`),
      enrollments: this.http.get<any[]>(`${this.apiUrl}/Enrollment`),
      categories: this.http.get<ICategory[]>(`${this.apiUrl}/Category`)
    }).pipe(
      map(data => {
        const mappedEnrollments = data.enrollments.map(e => this.mapEnrollmentFromBackend(e));
        // Calculate total students from unique student IDs
        const uniqueStudentIds = new Set(mappedEnrollments.map(e => e.studentId));
        const totalStudents = uniqueStudentIds.size;
        const activeStudents = new Set(mappedEnrollments.filter(e => e.status === 'Active').map(e => e.studentId)).size;
        
        return {
          totalCourses: data.courses.length,
          totalInstructors: data.instructors.length,
          totalEnrollments: mappedEnrollments.length,
          totalCategories: data.categories.length,
          activeStudents: activeStudents,
          pendingInstructors: data.instructors.filter(i => !i.isVerified).length,
          totalStudents: totalStudents
        };
      })
    );
  }

  getAnalytics(): Observable<IAnalytics> {
    return forkJoin({
      courses: this.http.get<ICourse[]>(`${this.apiUrl}/Course`),
      instructors: this.http.get<IInstructor[]>(`${this.apiUrl}/Instructor`),
      enrollments: this.http.get<any[]>(`${this.apiUrl}/Enrollment`),
      categories: this.http.get<ICategory[]>(`${this.apiUrl}/Category`)
    }).pipe(
      map(data => {
        const mappedEnrollments = data.enrollments.map(e => this.mapEnrollmentFromBackend(e));
        const recentEnrollments = mappedEnrollments
          .sort((a, b) => new Date(b.enrollmentDate).getTime() - new Date(a.enrollmentDate).getTime())
          .slice(0, 10);

        const enrollmentsByMonth = this.calculateEnrollmentsByMonth(mappedEnrollments);
        const coursesByCategory = this.calculateCoursesByCategory(data.courses, data.categories);

        // Calculate total students from unique student IDs in enrollments
        const uniqueStudentIds = new Set(mappedEnrollments.map(e => e.studentId));
        const totalStudents = uniqueStudentIds.size;
        const activeStudents = new Set(mappedEnrollments.filter(e => e.status === 'Active').map(e => e.studentId)).size;

        return {
          totalCourses: data.courses.length,
          totalInstructors: data.instructors.length,
          totalEnrollments: mappedEnrollments.length,
          totalCategories: data.categories.length,
          activeStudents: activeStudents,
          pendingInstructors: data.instructors.filter(i => !i.isVerified).length,
          totalStudents: totalStudents,
          recentEnrollments,
          enrollmentsByMonth,
          coursesByCategory
        };
      })
    );
  }

  private calculateEnrollmentsByMonth(enrollments: IEnrollment[]) {
    const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const monthData: { [key: string]: { enrollments: number; completions: number } } = {};

    enrollments.forEach(enrollment => {
      const date = new Date(enrollment.enrollmentDate);
      const monthKey = `${monthNames[date.getMonth()]} ${date.getFullYear()}`;

      if (!monthData[monthKey]) {
        monthData[monthKey] = { enrollments: 0, completions: 0 };
      }

      monthData[monthKey].enrollments++;
      if (enrollment.status === 'Completed') {
        monthData[monthKey].completions++;
      }
    });

    return Object.entries(monthData)
      .map(([month, data]) => ({
        month,
        enrollments: data.enrollments,
        completions: data.completions
      }))
      .slice(-6);
  }

  private calculateCoursesByCategory(courses: ICourse[], categories: ICategory[]) {
    const categoryMap: { [key: number]: number } = {};

    courses.forEach(course => {
      categoryMap[course.categoryId] = (categoryMap[course.categoryId] || 0) + 1;
    });

    return categories.map(category => ({
      categoryName: category.name,
      coursesCount: categoryMap[category.id] || 0
    })).filter(c => c.coursesCount > 0);
  }

  // ==================== COURSE MANAGEMENT ====================

  getCourses(): Observable<ICourse[]> {
    return this.http.get<ICourse[]>(`${this.apiUrl}/Course`);
  }

  getCourseById(id: number): Observable<ICourse> {
    return this.http.get<ICourse>(`${this.apiUrl}/Course/${id}`);
  }

  createCourse(course: ICourseCreate, thumbnailFile?: File): Observable<ICourse> {
    const formData = new FormData();
    formData.append('Title', course.title);
    formData.append('Description', course.description || '');
    formData.append('Price', course.price.toString());
    formData.append('IsFree', course.isFree.toString());
    formData.append('CategoryId', course.categoryId.toString());
    formData.append('InstructorId', course.instructorId.toString());
    
    if (thumbnailFile) {
      formData.append('ThumbnailUrl', thumbnailFile);
    } else if (course.thumbnailUrl) {
      // If URL is provided but no file, we'll send it as a string
      // Note: Backend expects IFormFile, so this might need adjustment
      formData.append('ThumbnailUrl', course.thumbnailUrl);
    }

    return this.http.post<ICourse>(`${this.apiUrl}/Course`, formData);
  }

  updateCourse(id: number, course: ICourseUpdate, thumbnailFile?: File): Observable<ICourse> {
    const formData = new FormData();
    
    if (course.title) formData.append('Title', course.title);
    if (course.description !== undefined) formData.append('Description', course.description);
    if (course.price !== undefined) formData.append('Price', course.price.toString());
    if (course.isFree !== undefined) formData.append('IsFree', course.isFree.toString());
    if (course.categoryId) formData.append('CategoryId', course.categoryId.toString());
    if (course.instructorId) formData.append('InstructorId', course.instructorId.toString());
    
    if (thumbnailFile) {
      formData.append('ThumbnailUrl', thumbnailFile);
    } else if (course.thumbnailUrl) {
      formData.append('ThumbnailUrl', course.thumbnailUrl);
    }

    return this.http.put<ICourse>(`${this.apiUrl}/Course/${id}`, formData);
  }

  deleteCourse(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Course/${id}`);
  }

  // ==================== INSTRUCTOR MANAGEMENT ====================

  getInstructors(): Observable<IInstructor[]> {
    return this.http.get<IInstructor[]>(`${this.apiUrl}/Instructor`);
  }

  getInstructorById(id: number): Observable<IInstructor> {
    return this.http.get<IInstructor>(`${this.apiUrl}/Instructor/${id}`);
  }

  registerInstructor(instructor: IInstructorRegister): Observable<IInstructor> {
    return this.http.post<IInstructor>(`${this.apiUrl}/Instructor/register-new`, instructor);
  }

  verifyInstructor(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/Instructor/verify/${id}`, {});
  }

  // ==================== ENROLLMENT MANAGEMENT ====================

  getEnrollments(): Observable<IEnrollment[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Enrollment`).pipe(
      map(enrollments => enrollments.map(e => this.mapEnrollmentFromBackend(e)))
    );
  }

  getEnrollmentsByStudent(studentId: number): Observable<IEnrollment[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Enrollment/GetEnrollmentsByStudentId/${studentId}`).pipe(
      map(enrollments => enrollments.map(e => this.mapEnrollmentFromBackend(e)))
    );
  }

  getEnrollmentsByInstructor(instructorId: number): Observable<IEnrollment[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Enrollment/GetEnrollmentsByInstructorId/${instructorId}`).pipe(
      map(enrollments => enrollments.map(e => this.mapEnrollmentFromBackend(e)))
    );
  }

  addEnrollment(enrollment: IEnrollmentCreate): Observable<IEnrollment> {
    // Backend expects StdId as int, not string
    const backendEnrollment = {
      StdId: parseInt(enrollment.studentId) || 0,
      CourseId: enrollment.courseId
    };
    return this.http.post<any>(`${this.apiUrl}/Enrollment/AddEnrollment`, backendEnrollment).pipe(
      map(e => this.mapEnrollmentFromBackend(e))
    );
  }

  cancelEnrollment(enrollmentId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/Enrollment/CancelEnrollment/${enrollmentId}`, {});
  }

  deleteEnrollment(enrollmentId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/Enrollment/DeleteEnrollment/${enrollmentId}`, {});
  }

  private mapEnrollmentFromBackend(e: any): IEnrollment {
    // Extract student name from Student object (which has User relation)
    let studentName = '';
    if (e.student?.user) {
      studentName = `${e.student.user.firstName || ''} ${e.student.user.lastName || ''}`.trim() || 
                    e.student.user.userName || 
                    e.student.user.email || '';
    } else if (e.Student?.User) {
      studentName = `${e.Student.User.FirstName || ''} ${e.Student.User.LastName || ''}`.trim() || 
                    e.Student.User.UserName || 
                    e.Student.User.Email || '';
    } else if (e.student?.name) {
      studentName = e.student.name;
    } else if (e.Student?.name) {
      studentName = e.Student.name;
    } else if (e.student?.userName) {
      studentName = e.student.userName;
    } else if (e.Student?.UserName) {
      studentName = e.Student.UserName;
    }

    return {
      enrollmentId: e.id || e.Id || 0,
      studentId: (e.stdId || e.StdId || '').toString(),
      courseId: e.courseId || e.CourseId || 0,
      enrollmentDate: e.enrollmentDate || e.EnrollmentDate || new Date(),
      status: (e.status || e.Status || 'Active') as 'Active' | 'Completed' | 'Cancelled',
      progress: e.progressPercentage || e.ProgressPercentage || 0,
      courseName: e.course?.title || e.Course?.Title || '',
      studentName: studentName || `Student #${e.stdId || e.StdId || 'Unknown'}`,
      instructorId: e.course?.instructorId || e.Course?.InstructorId
    };
  }

  // ==================== MODULE MANAGEMENT ====================

  getModules(): Observable<IModule[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Module`).pipe(
      map(modules => modules.map(m => this.mapModuleFromBackend(m)))
    );
  }

  getModulesByCourse(courseId: number): Observable<IModule[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Module/GetModulesByCrsID/${courseId}`).pipe(
      map(modules => modules.map(m => this.mapModuleFromBackend(m)))
    );
  }

  createModule(module: IModuleCreate): Observable<IModule> {
    // Backend expects: Id, Title, ModuleArrangement, CourseId
    const backendModule = {
      Id: 0, // New module, no ID yet
      Title: module.moduleName,
      ModuleArrangement: module.moduleOrder,
      CourseId: module.courseId
    };
    return this.http.post<any>(`${this.apiUrl}/Module`, backendModule).pipe(
      map(m => this.mapModuleFromBackend(m))
    );
  }

  updateModule(module: IModuleUpdate): Observable<IModule> {
    // Backend expects AddModuleDTO: Id, Title, ModuleArrangement, CourseId
    if (!module.courseId) {
      throw new Error('courseId is required for module update');
    }
    const backendModule = {
      Id: module.moduleId,
      Title: module.moduleName || '',
      ModuleArrangement: module.moduleOrder || 0,
      CourseId: module.courseId
    };
    return this.http.put<any>(`${this.apiUrl}/Module`, backendModule).pipe(
      map(m => this.mapModuleFromBackend(m))
    );
  }

  deleteModule(moduleId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Module?id=${moduleId}`);
  }

  private mapModuleFromBackend(m: any): IModule {
    return {
      moduleId: m.id || m.Id || 0,
      courseId: m.courseId || m.CourseId || 0,
      moduleName: m.title || m.Title || '',
      moduleDescription: '', // Backend doesn't return description
      moduleOrder: m.moduleArrangement || m.ModuleArrangement || 0,
      videosCount: m.videos?.length || m.Videos?.length || 0
    };
  }

  // ==================== VIDEO MANAGEMENT ====================

  getVideos(): Observable<IVideo[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Video/GetAllVideos`).pipe(
      map(videos => videos.map(v => this.mapVideoFromBackend(v)))
    );
  }

  uploadVideo(videoFile: File, title: string, moduleId: number, videoOrder: number): Observable<IVideo> {
    const formData = new FormData();
    formData.append('File', videoFile);
    formData.append('Title', title);
    formData.append('ModuleId', moduleId.toString());
    formData.append('VideoArrangement', videoOrder.toString());

    // Backend returns { message: "Video uploaded successfully" }, not the video object
    // So we need to fetch the video list and find the newly uploaded one, or return a placeholder
    return this.http.post<any>(`${this.apiUrl}/Video/upload`, formData).pipe(
      // After upload, we could reload videos, but for now return a placeholder
      map(() => ({
        id: 0,
        moduleId: moduleId,
        title: title,
        videoUrl: '',
        videoOrder: videoOrder,
        duration: 0
      } as IVideo))
    );
  }

  updateVideo(video: IVideoUpdate): Observable<IVideo> {
    // Backend expects VideoUpdateDto with File (optional), Title, VideoArrangement
    const formData = new FormData();
    if (video.title) formData.append('Title', video.title);
    if (video.videoOrder) formData.append('VideoArrangement', video.videoOrder.toString());
    if (video.id) formData.append('Id', video.id.toString());
    
    return this.http.put<any>(`${this.apiUrl}/Video/UpdateVideo`, formData).pipe(
      map(v => this.mapVideoFromBackend(v))
    );
  }

  deleteVideo(videoId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Video/DeleteVideo/${videoId}`);
  }

  private mapVideoFromBackend(v: any): IVideo {
    return {
      id: v.id || v.Id || 0,
      moduleId: v.moduleId || v.ModuleId || 0,
      title: v.title || v.Title || '',
      videoUrl: v.videoUrl || v.VideoUrl || v.filePath || v.FilePath || '',
      duration: v.duration || v.Duration || 0,
      videoOrder: v.videoOrder || v.VideoOrder || v.videoArrangement || v.VideoArrangement || 0,
      moduleName: v.moduleName || v.ModuleName || v.courseName || v.CourseName || ''
    };
  }

  // ==================== CATEGORY MANAGEMENT ====================

  getCategories(): Observable<ICategory[]> {
    return this.http.get<ICategory[]>(`${this.apiUrl}/Category`);
  }

  getCategoryById(id: number): Observable<ICategory> {
    return this.http.get<ICategory>(`${this.apiUrl}/Category/${id}`);
  }

  createCategory(category: ICategoryCreate): Observable<ICategory> {
    return this.http.post<ICategory>(`${this.apiUrl}/Category`, category);
  }

  updateCategory(id: number, category: ICategoryUpdate): Observable<ICategory> {
    return this.http.put<ICategory>(`${this.apiUrl}/Category/${id}`, category);
  }

  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Category/${id}`);
  }

  // ==================== STUDENT MANAGEMENT ====================
  // Note: Student management is handled through the Auth system
  // Students are users with the 'Student' role
}
