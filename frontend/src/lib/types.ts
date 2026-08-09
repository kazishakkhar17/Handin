export type UserRole = "Admin" | "Teacher" | "Student";

export interface UserResponse {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
  isActive: boolean;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: UserResponse;
}

export interface ClassCourseResponse {
  id: number;
  name: string;
  description?: string | null;
}

export interface SubjectResponse {
  id: number;
  name: string;
  code?: string | null;
  classCourseId: number;
  classCourseName: string;
}

export type AssignmentStatus = "Draft" | "Published";
export type SubmissionStatus = "Submitted" | "Late" | "Graded" | "ReturnedForRevision";

export interface AssignmentResponse {
  id: number;
  title: string;
  description: string;
  subjectId: number;
  subjectName: string;
  classCourseId: number;
  classCourseName: string;
  teacherId: number;
  teacherName: string;
  deadline: string;
  maxMarks: number;
  status: AssignmentStatus;
  allowResubmission: boolean;
  createdAt: string;
  mySubmissionStatus?: string | null;
}

export interface SubmissionResponse {
  id: number;
  assignmentId: number;
  assignmentTitle: string;
  studentId: number;
  studentName: string;
  answerText: string;
  attachmentUrl?: string | null;
  submittedAt: string;
  lastUpdatedAt?: string | null;
  status: SubmissionStatus;
  marksAwarded?: number | null;
  feedback?: string | null;
  gradedAt?: string | null;
}

export interface TeachingAssignmentResponse {
  subjectId: number;
  subjectName: string;
  classCourseId: number;
  classCourseName: string;
}

export interface ApiError {
  error: string;
}
