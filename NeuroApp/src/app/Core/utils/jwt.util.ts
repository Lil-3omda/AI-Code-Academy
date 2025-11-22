import { JwtHelperService } from '@auth0/angular-jwt';

export interface JwtPayload {
  sub: string;
  entityId?: string;  // Student/Instructor ID from token
  role: string;
  email?: string;
  name?: string;
  exp: number;
  iat: number;
}

/**
 * Utility to extract entityId from JWT token stored in localStorage
 */
export class JwtUtil {
  private static jwtHelper = new JwtHelperService();
  private static readonly TOKEN_KEY = 'token';

  /**
   * Get the JWT token from localStorage
   */
  static getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  /**
   * Decode the JWT token and return the payload
   */
  static decodeToken(): JwtPayload | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    try {
      return this.jwtHelper.decodeToken<JwtPayload>(token);
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  /**
   * Get entity ID (StudentId / InstructorId) from JWT token
   * This is the ID used for enrollment checks
   */
  static getEntityId(): string | null {
    const decoded = this.decodeToken();
    return decoded?.entityId || null;
  }

  /**
   * Get user role from JWT token
   */
  static getUserRole(): string | null {
    const decoded = this.decodeToken();
    return decoded?.role || null;
  }

  /**
   * Check if the JWT token is expired
   */
  static isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) {
      return true;
    }

    try {
      return this.jwtHelper.isTokenExpired(token);
    } catch (error) {
      console.error('Error checking token expiration:', error);
      return true;
    }
  }

  /**
   * Check if the user is authenticated (has valid, non-expired token)
   */
  static isAuthenticated(): boolean {
    const token = this.getToken();
    return token !== null && !this.isTokenExpired();
  }
}


