import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectorRef,
  ViewChild,
  ElementRef,
  AfterViewInit
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import {
  CoursePlayerService,
  CourseDetailsDto,
  ModulesDto,
  VideoGetAllDto,
  VideoDto
} from '../../Services/course-player.service';
import { JwtUtil } from '../../../../..//Core/utils/jwt.util';
import { Navbar } from '../../../../Home/components/navbar/navbar/navbar';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-course-player',
  standalone: true,
  imports: [CommonModule, Navbar],
  templateUrl: './course-player.component.html',
  styleUrls: ['./course-player.component.scss']
})
export class CoursePlayerComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('videoPlayer', { static: false }) videoPlayer!: ElementRef<HTMLVideoElement>;
  @ViewChild('videoContainer', { static: false }) videoContainer!: ElementRef<HTMLDivElement>;
  @ViewChild('watermark', { static: false }) watermark!: ElementRef<HTMLDivElement>;

  courseId!: number;
  course: CourseDetailsDto | null = null;
  modules: ModulesDto[] = [];
  videos: VideoGetAllDto[] = [];
  selectedModule: ModulesDto | null = null;
  selectedVideo: VideoGetAllDto | VideoDto | null = null;

  isEnrolled = false;
  isLoading = true;
  enrollmentCheckComplete = false;
  errorMessage = '';

  // Video player state
  currentVideoUrl = '';
  isPlaying = false;
  currentTime = 0;
  duration = 0;
  volume = 1;
  isFullscreen = false;

  // Watermark
  watermarkInterval: any;
  entityId: string | null = null;

  // internal
  private subscriptions: Subscription[] = [];
  private timeUpdateHandler!: () => void;
  private playHandler!: () => void;
  private pauseHandler!: () => void;
  private loadedMetaHandler!: () => void;
  private endedHandler!: () => void;
  private progressSaverInterval: any;
  private beforeUnloadHandler!: () => void;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private coursePlayerService: CoursePlayerService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Get entityId from JWT
    this.entityId = JwtUtil.getEntityId();

    // Check authentication
    if (!JwtUtil.isAuthenticated() || !this.entityId) {
      this.errorMessage = 'You must be logged in to view this course';
      this.isLoading = false;
      this.enrollmentCheckComplete = true;
      return;
    }

    // load saved volume
    const savedVolume = localStorage.getItem('course-player-volume');
    if (savedVolume !== null) {
      const v = Number(savedVolume);
      if (!isNaN(v)) this.volume = Math.max(0, Math.min(1, v));
    }

    // Subscribe to route parameter changes
    const sub = this.route.paramMap.subscribe(params => {
      const courseIdParam = params.get('courseId') || params.get('id');
      if (!courseIdParam) {
        this.errorMessage = 'Course ID is required';
        this.isLoading = false;
        return;
      }

      const newCourseId = parseInt(courseIdParam, 10);
      if (isNaN(newCourseId)) {
        this.errorMessage = 'Invalid course ID';
        this.isLoading = false;
        return;
      }

      const videoIdParam = params.get('videoId');
      const videoId = videoIdParam ? parseInt(videoIdParam, 10) : null;

      if (newCourseId !== this.courseId) {
        this.courseId = newCourseId;
        this.loadCourseData(videoId);
      } else if (videoId && this.videos.length > 0) {
        this.selectVideoById(videoId);
      }
    });
    this.subscriptions.push(sub);
  }

  ngAfterViewInit(): void {
    // Initial setup if element available
    if (this.videoPlayer) {
      this.setupVideoPlayer();
      // apply volume to element
      try {
        this.videoPlayer.nativeElement.volume = this.volume;
      } catch {}
    }
    this.setupWatermark();

    // save progress on leave
    this.beforeUnloadHandler = () => this.saveProgress();
    window.addEventListener('beforeunload', this.beforeUnloadHandler);
    document.addEventListener('fullscreenchange', () => {
      this.isFullscreen = !!document.fullscreenElement;
      this.cdr.detectChanges();
    });
  }

  ngOnDestroy(): void {
    // clear intervals/listeners
    if (this.watermarkInterval) clearInterval(this.watermarkInterval);
    if (this.progressSaverInterval) clearInterval(this.progressSaverInterval);
    window.removeEventListener('beforeunload', this.beforeUnloadHandler);

    // remove video listeners
    this.detachVideoListeners();

    this.subscriptions.forEach(s => s.unsubscribe());
  }

  // ---------- Data loading ----------
  loadCourseData(videoId?: number | null): void {
    this.isLoading = true;
    const studentId = parseInt(this.entityId!, 10);

    if (isNaN(studentId)) {
      this.errorMessage = 'Invalid student ID';
      this.isLoading = false;
      this.enrollmentCheckComplete = true;
      return;
    }

    this.coursePlayerService.checkEnrollment(studentId, this.courseId).subscribe({
      next: (enrolled) => {
        this.isEnrolled = enrolled;
        this.enrollmentCheckComplete = true;

        if (!enrolled) {
          this.errorMessage = 'You are not enrolled in this course. Please enroll to access the content.';
          this.isLoading = false;
          this.cdr.detectChanges();
          return;
        }

        this.coursePlayerService.getCourseWithContent(this.courseId).subscribe({
          next: (data) => {
            this.course = data.course;
            this.modules = data.modules || [];
            this.videos = data.videos || [];

            // Organize videos by module and attach saved progress
            this.modules.forEach(module => {
              if (!module.videos) module.videos = [];
              const moduleVideos = this.videos
                .filter(v => v.moduleId === module.id)
                .sort((a, b) => a.videoArrangement - b.videoArrangement);
              // assign progress from storage
              moduleVideos.forEach(v => {
                const saved = this.getSavedProgress(v.id);
                (v as any).progress = saved ?? 0;
              });
              module.videos = moduleVideos;
            });

            // choose video
            if (videoId) {
              const targetVideo = this.videos.find(v => v.id === videoId);
              if (targetVideo) {
                const targetModule = this.modules.find(m => m.id === targetVideo.moduleId);
                if (targetModule) {
                  this.selectModule(targetModule);
                  this.selectVideo(targetVideo);
                } else if (this.modules.length > 0) {
                  this.selectModule(this.modules[0]);
                }
              } else if (this.modules.length > 0) {
                this.selectModule(this.modules[0]);
              }
            } else if (this.modules.length > 0) {
              this.selectModule(this.modules[0]);
            }

            this.isLoading = false;
            this.cdr.detectChanges();
          },
          error: (error) => {
            console.error('Error loading course content:', error);
            this.errorMessage = 'Failed to load course content. Please try again later.';
            this.isLoading = false;
            this.cdr.detectChanges();
          }
        });
      },
      error: (error) => {
        console.error('Error checking enrollment:', error);
        this.errorMessage = 'Failed to verify enrollment. Please try again later.';
        this.isLoading = false;
        this.enrollmentCheckComplete = true;
        this.cdr.detectChanges();
      }
    });
  }

  // ---------- Module & video selection ----------
  selectModule(module: ModulesDto): void {
    this.selectedModule = module;
    if (module.videos && module.videos.length > 0) {
      const firstVideo = module.videos[0];
      this.selectVideo(firstVideo);
    } else {
      this.selectedVideo = null;
      this.currentVideoUrl = '';
      this.cdr.detectChanges();
    }
  }

  selectVideo(video: VideoGetAllDto | VideoDto): void {
    if (!this.isEnrolled) return;

    // set selected
    this.selectedVideo = video;
    const videoUrl = this.coursePlayerService.getVideoUrl((video as any).filePath);
    this.currentVideoUrl = videoUrl;

    // scroll selected into view in sidebar
    setTimeout(() => {
      const activeItem = document.querySelector('.video-item.active');
      activeItem?.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }, 60);

    // update route (so deep-link works)
    this.router.navigate(['/courses', this.courseId, 'video', (video as any).id], {
      replaceUrl: true
    });

    // detach old listeners and re-attach after load
    this.detachVideoListeners();

    // small delay to let binding update & video element reload
    setTimeout(() => {
      if (this.videoPlayer) {
        try {
          this.videoPlayer.nativeElement.load();
        } catch {}
        // attach listeners (will also handle loadedmetadata to restore position)
        this.setupVideoPlayer();
      }
    }, 120);

    this.cdr.detectChanges();
  }

  selectVideoById(videoId: number): void {
    const targetVideo = this.videos.find(v => v.id === videoId);
    if (targetVideo) {
      const targetModule = this.modules.find(m => m.id === targetVideo.moduleId);
      if (targetModule) {
        this.selectedModule = targetModule;
        this.selectVideo(targetVideo);
      }
    }
  }

  // ---------- Video player setup & listeners ----------
  setupVideoPlayer(): void {
    if (!this.videoPlayer) return;
    const videoEl = this.videoPlayer.nativeElement;

    // define handlers as arrow functions so they can be removed later
    this.timeUpdateHandler = () => {
      this.currentTime = videoEl.currentTime || 0;
      this.duration = videoEl.duration || 0;
      // update progress for selected video object
      if (this.selectedVideo) {
        (this.selectedVideo as any).progress = this.currentTime;
      }
      this.cdr.detectChanges();
    };

    this.playHandler = () => {
      this.isPlaying = true;
      this.cdr.detectChanges();
    };

    this.pauseHandler = () => {
      this.isPlaying = false;
      this.cdr.detectChanges();
    };

    this.loadedMetaHandler = () => {
      this.duration = videoEl.duration || 0;
      // restore saved progress position for current video
      const vidId = (this.selectedVideo as any)?.id;
      const saved = this.getSavedProgress(vidId);
      if (saved && !isNaN(saved) && saved > 0 && saved < this.duration - 1) {
        try {
          videoEl.currentTime = saved;
        } catch {}
      }
      // set volume
      try {
        videoEl.volume = this.volume;
      } catch {}
      this.cdr.detectChanges();
    };

    this.endedHandler = () => {
      // mark as completed (set progress to duration)
      if (this.selectedVideo) {
        (this.selectedVideo as any).progress = this.duration;
        this.saveProgress();
      }
      this.isPlaying = false;
      this.cdr.detectChanges();
    };

    // add event listeners
    videoEl.addEventListener('timeupdate', this.timeUpdateHandler);
    videoEl.addEventListener('play', this.playHandler);
    videoEl.addEventListener('pause', this.pauseHandler);
    videoEl.addEventListener('loadedmetadata', this.loadedMetaHandler);
    videoEl.addEventListener('ended', this.endedHandler);

    // start periodic saver (every 5 seconds)
    if (this.progressSaverInterval) clearInterval(this.progressSaverInterval);
    this.progressSaverInterval = setInterval(() => {
      this.saveProgress();
    }, 5000);
  }

  detachVideoListeners(): void {
    if (!this.videoPlayer) return;
    const videoEl = this.videoPlayer.nativeElement;
    try {
      if (this.timeUpdateHandler) videoEl.removeEventListener('timeupdate', this.timeUpdateHandler);
      if (this.playHandler) videoEl.removeEventListener('play', this.playHandler);
      if (this.pauseHandler) videoEl.removeEventListener('pause', this.pauseHandler);
      if (this.loadedMetaHandler) videoEl.removeEventListener('loadedmetadata', this.loadedMetaHandler);
      if (this.endedHandler) videoEl.removeEventListener('ended', this.endedHandler);
    } catch {}
    if (this.progressSaverInterval) {
      clearInterval(this.progressSaverInterval);
      this.progressSaverInterval = null;
    }
  }

  // ---------- Controls ----------
  togglePlay(): void {
    if (!this.videoPlayer) return;
    const el = this.videoPlayer.nativeElement;
    try {
      if (el.paused) {
        const playPromise = el.play();
        if (playPromise && typeof playPromise.then === 'function') {
          playPromise.catch(() => {});
        }
      } else {
        el.pause();
      }
    } catch {}
  }

  toggleFullscreen(): void {
    const container = this.videoContainer?.nativeElement || this.videoPlayer?.nativeElement.parentElement;
    if (!container) return;
    if (!document.fullscreenElement) {
      container.requestFullscreen?.();
    } else {
      document.exitFullscreen?.();
    }
  }

  onProgressBarClick(event: MouseEvent): void {
    if (!this.videoPlayer) return;
    const container = (event.currentTarget as HTMLElement);
    const rect = container.getBoundingClientRect();
    const clickX = event.clientX - rect.left;
    const pct = clickX / rect.width;
    const seek = (this.duration || 0) * pct;
    this.seekTo(seek);
  }

  seekTo(time: number): void {
    if (!this.videoPlayer) return;
    try {
      this.videoPlayer.nativeElement.currentTime = Math.max(0, Math.min(time, this.duration || time));
    } catch {}
  }

  setVolume(volume: number | string): void {
    const v = typeof volume === 'string' ? parseFloat(volume) : volume;
    this.volume = Math.max(0, Math.min(1, Number(v)));
    if (this.videoPlayer) {
      try {
        this.videoPlayer.nativeElement.volume = this.volume;
      } catch {}
    }
    localStorage.setItem('course-player-volume', String(this.volume));
    this.cdr.detectChanges();
  }

  // ---------- Progress persistence ----------
  saveProgress(): void {
    const vidId = (this.selectedVideo as any)?.id;
    if (!vidId || !this.videoPlayer) return;
    try {
      const time = this.videoPlayer.nativeElement.currentTime || 0;
      localStorage.setItem(`video-progress-${vidId}`, String(time));
      // also update object's progress
      (this.selectedVideo as any).progress = time;
    } catch {}
  }

  getSavedProgress(videoId: number | undefined | null): number | null {
    if (!videoId) return null;
    const raw = localStorage.getItem(`video-progress-${videoId}`);
    if (!raw) return null;
    const num = Number(raw);
    return isNaN(num) ? null : num;
  }

  // ---------- Watermark ----------
  setupWatermark(): void {
    if (!this.watermark || !this.entityId) return;

    // Move watermark randomly every 2.5 seconds with smoother transitions
    if (this.watermarkInterval) clearInterval(this.watermarkInterval);
    this.watermarkInterval = setInterval(() => {
      if (this.watermark) {
        const container = this.watermark.nativeElement.parentElement;
        if (container) {
          const maxX = Math.max(0, container.offsetWidth - this.watermark.nativeElement.offsetWidth - 10);
          const maxY = Math.max(0, container.offsetHeight - this.watermark.nativeElement.offsetHeight - 10);

          const randomX = Math.random() * maxX;
          const randomY = Math.random() * maxY;

          this.watermark.nativeElement.style.left = `${randomX}px`;
          this.watermark.nativeElement.style.top = `${randomY}px`;
          this.watermark.nativeElement.style.opacity = '0.6';
        }
      }
    }, 2500);
  }

  // ---------- Utilities ----------
  formatTime(seconds: number): string {
    if (!seconds || isNaN(seconds)) return '0:00';
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  getWatermarkText(): string {
    const timestamp = new Date().toLocaleString();
    return `User: ${this.entityId} • ${timestamp}`;
  }

  goToEnrollment(): void {
    this.router.navigate(['/CourseDetails', this.courseId]);
  }

  getModuleVideos(moduleId: number): VideoGetAllDto[] {
    return this.videos
      .filter(v => v.moduleId === moduleId)
      .sort((a, b) => a.videoArrangement - b.videoArrangement);
  }
}
